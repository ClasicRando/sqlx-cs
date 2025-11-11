using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Cache;
using Sqlx.Core.Config;
using Sqlx.Core.Connection;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Notify;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Stream;

/// <summary>
/// Underlining connection to a Postgres database backend. Performs all the IO operations through an
/// <see cref="IAsyncStream"/>.
/// </summary>
internal sealed partial class PgStream : IDisposable
{
    private const string BeginQuery = "BEGIN;";
    private const string CommitQuery = "COMMIT;";
    private const string RollbackQuery = "ROLLBACK;";
    
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    private bool _disposed;
    private readonly IAsyncStream _asyncStream;
    private PgConnectionPool? _pool;
    private readonly ILogger<PgStream> _logger;
    private readonly WriteBuffer _writeBuffer = new();
    private readonly Channel<PgNotification> _notifications = Channel.CreateUnbounded<PgNotification>();
    private BackendDataKeyMessage? _backendDataKey;
    private long _inTransaction;
    private int _pendingReadyForQuery;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    internal PgStream(IAsyncStream asyncStream, PgConnectionPool pool)
    {
        _asyncStream = asyncStream;
        _pool = pool;
        _logger = _pool.ConnectOptions.LoggerFactory.CreateLogger<PgStream>();
        _statementCache = new LruCache<string, PgPreparedStatement>(
            pool.ConnectOptions.StatementCacheCapacity);
    }

    private PgConnectOptions ConnectOptions => _pool!.ConnectOptions;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Closed;
    
    public bool InTransaction => Interlocked.Read(ref _inTransaction) == 1;
    
    /// <summary>
    /// Open the underlining stream and initiate the connection with the database backend 
    /// </summary>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    internal async Task OpenAsync(CancellationToken cancellationToken)
    {
        CheckDisposed();
        Status = ConnectionStatus.Connecting;
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _asyncStream.OpenAsync(
                    ConnectOptions.Host,
                    ConnectOptions.Port,
                    cancellationToken)
                .ConfigureAwait(false);
            await SendStartupMessage(ConnectOptions, cancellationToken).ConfigureAwait(false);
            await HandleAuthFlow(cancellationToken).ConfigureAwait(false);
            await WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            Status = ConnectionStatus.Idle;
        }
        catch
        {
            Status = ConnectionStatus.Broken;
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Receive the next message as an <see cref="IAuthMessage"/> which instructs the client on the
    /// auth mechanism/flow to follow.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <exception cref="PgException">
    /// If the auth flow specified by the database is not supported
    /// </exception>
    private async Task HandleAuthFlow(CancellationToken cancellationToken)
    {
        var authentication = await ReceiveNextMessageAs<IAuthMessage>(cancellationToken)
            .ConfigureAwait(false);
        switch (authentication)
        {
            case OkAuthMessage:
                // logged in!
                break;
            case ClearTextPasswordAuthMessage:
                ArgumentNullException.ThrowIfNull(ConnectOptions.Password);
                await SimplePasswordAuthFlow(
                    ConnectOptions.Username,
                    ConnectOptions.Password,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                break;
            case MD5PasswordAuthMessage md5PasswordAuthMessage:
                ArgumentNullException.ThrowIfNull(ConnectOptions.Password);
                await SimplePasswordAuthFlow(
                    ConnectOptions.Username,
                    ConnectOptions.Password,
                    salt: md5PasswordAuthMessage.Salt,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                break;
            case SaslAuthMessage saslAuthMessage:
                ArgumentNullException.ThrowIfNull(ConnectOptions.Password);
                await SaslAuthFlow(ConnectOptions.Password, saslAuthMessage, cancellationToken)
                    .ConfigureAwait(false);
                break;
            default:
                throw new PgException($"Auth request type cannot be handled. {authentication}");
        }
    }

    /// <summary>
    /// Check if the current connection is valid. Issues a simple query that should always succeed
    /// unless the connection is broken. If an exception is thrown or the query result is an error,
    /// the <see cref="Status"/> property will be updated to <see cref="ConnectionStatus.Broken"/>.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>True if the connection can query the database, otherwise false</returns>
    public async Task<bool> IsValidAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotOpen();
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            await SendQueryMessage("SELECT 1;", cancellationToken).ConfigureAwait(false);
            var result = await WaitForOrError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            if (result is Either<ReadyForQueryMessage, ErrorResponseMessage>.Right)
            {
                Status = ConnectionStatus.Broken;
                return false;
            }
        }
        catch (SqlxException)
        {
            Status = ConnectionStatus.Broken;
            return false;
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    /// <summary>
    /// Execute the query as either a simple query or extended query based upon the checks performed
    /// in <see cref="IsSimpleQuery"/>.
    /// </summary>
    /// <param name="query">Query to execute</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns></returns>
    /// <exception cref="PgException">
    /// If the supplied query is not a <see cref="PgExecutableQuery"/> or the connection is closed
    /// or broken.
    /// </exception>
    public IAsyncEnumerable<Either<IDataRow, QueryResult>> ExecuteQuery(
        IQuery query,
        CancellationToken cancellationToken)
    {
        PgExecutableQuery executableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        var results = IsSimpleQuery(executableQuery)
            ? SendSimpleQuery(executableQuery.Query, cancellationToken)
            : SendExtendedQuery(executableQuery.Query, executableQuery.ParameterBuffer, cancellationToken);
        return results;
    }

    public IAsyncEnumerable<Either<IDataRow, QueryResult>> ExecuteQueryBatch(
        IQueryBatch query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Call <see cref="OpenAsync"/> is the connection is closed.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private async ValueTask ConnectIfClosed(CancellationToken cancellationToken)
    {
        if (Status is not ConnectionStatus.Closed)
        {
            return;
        }

        await OpenAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <exception cref="InvalidOperationException">
    /// If the connection was disposed or <see cref="Status"/> value is
    /// <see cref="ConnectionStatus.Broken"/> or <see cref="ConnectionStatus.Closed"/>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfNotOpen()
    {
        CheckDisposed();
        if (Status is ConnectionStatus.Broken or ConnectionStatus.Closed)
        {
            throw new InvalidOperationException(
                "Attempted to perform operation with a connection that is not idle");
        }
    }

    /// <summary>
    /// Execute the desired transaction command. If an error occurs trying to commiting a
    /// transaction, a <c>ROLLBACK</c> command will be tried as a last effort to resolve the
    /// transaction state, avoid locks and keep consistency.
    /// </summary>
    /// <param name="transactionCommand">Transaction command</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <exception cref="UnexpectedTransactionState">
    /// If the transaction command would create an inconsistent state, such as attempting to:
    /// <list type="bullet">
    ///     <item>begin a new transaction while already within a transaction</item>
    ///     <item>commit or rollback a transaction while not within a transaction</item>
    /// </list>
    /// </exception>
    public async Task ExecuteTransactionCommand(
        AbstractConnection.TransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        ThrowIfNotOpen();
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            switch (transactionCommand)
            {
                case AbstractConnection.TransactionCommand.Begin when InTransaction:
                    throw new UnexpectedTransactionState(false);
                case AbstractConnection.TransactionCommand.Commit or AbstractConnection.TransactionCommand.Rollback when !InTransaction:
                    throw new UnexpectedTransactionState(true);
            }

            var sql = transactionCommand switch
            {
                AbstractConnection.TransactionCommand.Begin => BeginQuery,
                AbstractConnection.TransactionCommand.Commit => CommitQuery,
                AbstractConnection.TransactionCommand.Rollback => RollbackQuery,
                _ => throw PgException.EnumOutOfRange(transactionCommand),
            };
            await SendQueryMessage(sql, cancellationToken).ConfigureAwait(false);
            await WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken).ConfigureAwait(false);
            Interlocked.Exchange(
                ref _inTransaction,
                transactionCommand is AbstractConnection.TransactionCommand.Begin ? 1 : 0);
        }
        catch (OutOfMemoryException)
        {
            throw;
        }
        catch (UnexpectedTransactionState)
        {
            throw;
        }
        catch
        {
            if (transactionCommand is not AbstractConnection.TransactionCommand.Commit) throw;
            
            try
            {
                await SendQueryMessage(RollbackQuery, cancellationToken)
                    .ConfigureAwait(false);
                await WaitForOrError<ReadyForQueryMessage>(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Keep pulling messages from the connection stream until all pending
    /// <see cref="ReadyForQueryMessage"/>s have been processed
    /// </summary>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    private async Task WaitUntilReady(CancellationToken cancellationToken)
    {
        while (_pendingReadyForQuery > 0)
        {
            ReadyForQueryMessage message = await WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            HandleReadyForQuery(message);
        }
    }

    /// <summary>
    /// Decrement <see cref="_pendingReadyForQuery"/> and inspect the supplied message
    /// </summary>
    /// <param name="readyForQuery">message from server</param>
    private void HandleReadyForQuery(ReadyForQueryMessage readyForQuery)
    {
        if (--_pendingReadyForQuery < 0)
        {
            _logger.LogWarning("Received more ReadyForQuery messages than expected");
            _pendingReadyForQuery = 0;
        }

        if (readyForQuery.TransactionStatus is TransactionStatus.FailedTransaction)
        {
            _logger.LogWarning("Server reported failed transaction");
        }
    }

    private async Task<PgNotification> WaitForNotificationOrError(CancellationToken cancellationToken)
    {
        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = await ApplyStandardMessageProcessing(
                backendMessage,
                cancellationToken,
                handleNotification: false)
                .ConfigureAwait(false);
            if (postProcessMessage is PgNotification result)
            {
                return result;
            }
            
            _logger.LogIgnoreUnexpectedMessage(
                SqlxConfig.DetailedLoggingLevel,
                backendMessage);
        }
    }

    /// <summary>
    /// Process messages until <typeparamref name="T"/> is parsed or an error is sent by the
    /// database backend. Defers to <see cref="WaitForOrError"/> and checks the result for an error.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <typeparam name="T">Message type to wait for</typeparam>
    /// <returns>The message if received before an error</returns>
    /// <exception cref="PgException">If an error message is sent by the backend</exception>
    private async Task<T> WaitForOrThrowError<T>(CancellationToken cancellationToken)
        where T : IPgBackendMessage
    {
        var result = await WaitForOrError<T>(cancellationToken)
            .ConfigureAwait(false);
        return result switch
        {
            Either<T, ErrorResponseMessage>.Left left => left.Value,
            Either<T, ErrorResponseMessage>.Right right => throw new PgException(right.Value),
            _ => throw new PgException("Found another Either type. How weird?"),
        };
    }

    /// <summary>
    /// Receives messages until the message is of the desired type <typeparamref name="T"/> or an
    /// error. Other messages are filtered through <see cref="ApplyStandardMessageProcessing"/> and
    /// any other message retained after that is discarded. <see cref="ErrorResponseMessage"/>s are
    /// not immediately thrown but returned for the caller to handle.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <typeparam name="T">Message type to wait for</typeparam>
    /// <returns>One of the message if received before an error or that error</returns>
    private async Task<Either<T, ErrorResponseMessage>> WaitForOrError<T>(
        CancellationToken cancellationToken)
        where T : IPgBackendMessage
    {
        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = await ApplyStandardMessageProcessing(
                backendMessage,
                cancellationToken,
                throwOnError: false)
                .ConfigureAwait(false);
            switch (postProcessMessage)
            {
                case null:
                    continue;
                case ErrorResponseMessage errorResponse:
                    return new Either<T, ErrorResponseMessage>.Right(errorResponse);
                case T result:
                    return new Either<T, ErrorResponseMessage>.Left(result);
                default:
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessage);
                    break;
            }
        }
    }

    /// <summary>
    /// Handle standard messages and return null when the message was handled. This generally
    /// applies to asynchronous messages that should be handled when sent out of order from the
    /// database backend.
    /// </summary>
    /// <param name="message">Message to optionally process</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <param name="throwOnError">True if <see cref="ErrorResponseMessage"/> should throw</param>
    /// <param name="handleNotification">
    /// True if <see cref="PgNotification"/>s should be sent to <see cref="OnNotification"/>
    /// </param>
    /// <returns>The message if not processed, otherwise null</returns>
    /// <exception cref="PgException">
    /// If <paramref name="throwOnError"/> is true and the message was an error response
    /// </exception>
    private async Task<IPgBackendMessage?> ApplyStandardMessageProcessing(
        IPgBackendMessage message,
        CancellationToken cancellationToken,
        bool throwOnError = true,
        bool handleNotification = true)
    {
        switch (message)
        {
            case NoticeResponseMessage noticeResponse:
                OnNotice(noticeResponse);
                return null;
            case PgNotification notification:
                if (!handleNotification) return notification;
                await OnNotification(notification, cancellationToken).ConfigureAwait(false);
                return null;
            case BackendDataKeyMessage backendDataKey:
                OnBackendDataKey(backendDataKey);
                return null;
            case ParameterStatusMessage parameterStatus:
                OnParameterStatus(parameterStatus);
                return null;
            case NegotiateProtocolVersionMessage negotiateProtocolVersion:
                OnNegotiateProtocolVersion(negotiateProtocolVersion);
                return null;
            case ErrorResponseMessage errorResponse:
                return throwOnError ? throw new PgException(errorResponse) : message;
            default:
                return message;
        }
    }

    /// <summary>
    /// Log <c>NOTICE</c>
    /// </summary>
    /// <param name="noticeResponse"></param>
    private void OnNotice(NoticeResponseMessage noticeResponse)
    {
        _logger.LogNotice(SqlxConfig.DetailedLoggingLevel, noticeResponse);
    }

    /// <summary>
    /// Add notification to notifications channel
    /// </summary>
    /// <param name="notification">Notification sent from database backend</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    private ValueTask OnNotification(
        PgNotification notification,
        CancellationToken cancellationToken)
    {
        return _notifications.Writer.WriteAsync(notification, cancellationToken);
    }

    /// <summary>
    /// Capture the backend data key message for future usage
    /// </summary>
    private void OnBackendDataKey(BackendDataKeyMessage message)
    {
        _logger.LogBackendDataKey(SqlxConfig.DetailedLoggingLevel, message.ProcessId);
        _backendDataKey = message;
    }

    /// <summary>
    /// Log parameter status update
    /// </summary>
    private void OnParameterStatus(ParameterStatusMessage message)
    {
        _logger.LogParameterStatus(SqlxConfig.DetailedLoggingLevel, message.Name);
    }

    /// <summary>
    /// Log protocol negotiation message
    /// </summary>
    private void OnNegotiateProtocolVersion(NegotiateProtocolVersionMessage message)
    {
        _logger.LogNegotiateProtocolVersion(
            SqlxConfig.DetailedLoggingLevel,
            message.NewestMinorProtocolVersion,
            message.ProtocolOptionsNotRecognized);
    }

    /// <summary>
    /// Receive the next backend message as a <see cref="IPgBackendMessage"/>. We must use the
    /// interface because the backend might send asynchronous messages that we need to handle.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>The next message sent by the backend</returns>
    private async Task<IPgBackendMessage> ReceiveNextMessage(CancellationToken cancellationToken)
    {
        var format = (PgBackendMessageType)await _asyncStream.ReadByteAsync(cancellationToken).ConfigureAwait(false);
        var size = await _asyncStream.ReadIntAsync(cancellationToken).ConfigureAwait(false) - 4;
        var contents = ArrayPool.Rent(size);
        try
        {
            await _asyncStream.ReadBuffer(contents.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
            return ParseMessage(format, contents.AsSpan(0, size));
        }
        finally
        {
            ArrayPool.Return(contents);
        }
    }

    /// <summary>
    /// Call <see cref="ReceiveNextMessage"/> and check that the message is of the desired type.
    /// This is most applicable to auth processing since a defined flow is expected without
    /// asynchronous messages.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <typeparam name="T">Desired message type</typeparam>
    /// <returns>The next message sent by the backend as <typeparamref name="T"/></returns>
    /// <exception cref="PgException">If the message is an error or not the desired type</exception>
    private async Task<T> ReceiveNextMessageAs<T>(CancellationToken cancellationToken)
        where T : IPgBackendMessage
    {
        IPgBackendMessage message = await ReceiveNextMessage(cancellationToken).ConfigureAwait(false);
        return message switch
        {
            T result => result,
            ErrorResponseMessage errorResponse => throw new PgException(errorResponse),
            _ => throw new PgException(
                $"Expected {typeof(T)} message but found {message.GetType()}"),
        };
    }

    /// <summary>
    /// Write all content in <see cref="_writeBuffer"/> to the <see cref="_asyncStream"/> and reset
    /// the <see cref="_writeBuffer"/> for future writes.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private async Task Flush(CancellationToken cancellationToken)
    {
        await _asyncStream.WriteAsync(_writeBuffer.ReadableMemory, cancellationToken);
        _writeBuffer.Reset();
    }

    /// <summary>
    /// Parse the current message contents based upon the <see cref="PgBackendMessageType"/>
    /// </summary>
    /// <param name="messageType">Message type</param>
    /// <param name="contents">Message contents to parse (could be empty)</param>
    /// <returns>Backend message parsed</returns>
    /// <exception cref="PgException">If the message type is not supported or exepcted</exception>
    private static IPgBackendMessage ParseMessage(
        PgBackendMessageType messageType,
        Span<byte> contents)
    {
        ReadBuffer buffer = new(contents);
        return messageType switch
        {
            PgBackendMessageType.Authentication => AuthenticationMessage.Decode(buffer),
            PgBackendMessageType.BackendDataKey => BackendDataKeyMessage.Decode(buffer),
            PgBackendMessageType.BindComplete => BindCompleteMessage.Instance,
            PgBackendMessageType.CloseComplete => CloseCompleteMessage.Instance,
            PgBackendMessageType.CommandComplete => CommandCompleteMessage.Decode(buffer),
            PgBackendMessageType.CopyData => CopyDataMessage.Decode(buffer),
            PgBackendMessageType.CopyDone => CopyDoneMessage.Instance,
            PgBackendMessageType.CopyInResponse => CopyInResponseMessage.Decode(buffer),
            PgBackendMessageType.CopyOutResponse => CopyOutResponseMessage.Decode(buffer),
            PgBackendMessageType.CopyBothResponse => throw new PgException(
                "CopyBoth is not supported by this driver"),
            PgBackendMessageType.DataRow => DataRowMessage.Decode(buffer),
            PgBackendMessageType.EmptyQueryResponse => throw new PgException(
                "Empty query response packet should never be received"),
            PgBackendMessageType.ErrorResponse => ErrorResponseMessage.Decode(buffer),
            PgBackendMessageType.NegotiateProtocolVersion =>
                NegotiateProtocolVersionMessage.Decode(buffer),
            PgBackendMessageType.NoData => NoDataMessage.Instance,
            PgBackendMessageType.NoticeResponse => NoticeResponseMessage.Decode(buffer),
            PgBackendMessageType.NotificationResponse => PgNotification.Decode(buffer),
            PgBackendMessageType.ParameterDescription => ParameterDescriptionMessage.Decode(buffer),
            PgBackendMessageType.ParameterStatus => ParameterStatusMessage.Decode(buffer),
            PgBackendMessageType.ParseComplete => ParseCompleteMessage.Instance,
            PgBackendMessageType.PortalSuspend => PortalSuspendedMessage.Instance,
            PgBackendMessageType.ReadyForQuery => ReadyForQueryMessage.Decode(buffer),
            PgBackendMessageType.RowDescription => RowDescriptionMessage.Decode(buffer),
            _ => throw new PgException($"Expected backend message type but found '{messageType}'"),
        };
    }
    
    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>
    /// Clean up the stream for future usage. This is called by the connection pool to prepare the
    /// connection for usage by another caller.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    internal ValueTask CleanUp(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _pool = null;
        // If we can send messages to the server, sent the terminate command before closing
        if (Status is ConnectionStatus.Idle)
        {
            try
            {
                _writeBuffer.Reset();
                SendTerminate().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error closing PgStream");
            }
        }
        _writeBuffer.Dispose();
        _asyncStream.Dispose();
        _disposed = true;
    }
}
