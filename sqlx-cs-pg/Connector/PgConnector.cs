using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Cache;
using Sqlx.Core.Config;
using Sqlx.Core.Connection;
using Sqlx.Core.Connector;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Backend.Information;
using Sqlx.Postgres.Notify;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Connector;

/// <summary>
/// Underlining connection to a Postgres database backend. Performs all the IO operations through an
/// <see cref="IAsyncConnector"/>.
/// </summary>
public sealed partial class PgConnector : IPooledConnection
{
    private const string BeginQuery = "BEGIN;";
    private const string CommitQuery = "COMMIT;";
    private const string RollbackQuery = "ROLLBACK;";

    public DateTime LastOpenTimestamp { get; private set; }
    public Guid Id { get; } = Guid.NewGuid();
    private bool _disposed;
    private readonly IAsyncConnector _asyncConnector;
    private readonly ILogger<PgConnector> _logger;
    private PgAsyncResultSet? _currentResultSet;

    private BackendDataKeyMessage? _backendDataKey;
    private int _pendingReadyForQuery;

    internal PgConnector(IAsyncConnector asyncConnector, PgConnectOptions connectOptions)
    {
        ConnectOptions = connectOptions;
        _asyncConnector = asyncConnector;
        _logger = connectOptions.LoggerFactory.CreateLogger<PgConnector>();
        _statementCache = new LruCache<string, PgPreparedStatement>(
            connectOptions.StatementCacheCapacity);
    }

    internal PgConnectOptions ConnectOptions { get; }

    private PipeWriter Writer => _asyncConnector.Writer;

    private int _connectionStatus = (int)ConnectionStatus.Closed;
    public ConnectionStatus Status
    {
        get => (ConnectionStatus)_connectionStatus;
        private set
        {
            var newStatus = (int)value;

            if (newStatus == _connectionStatus)
            {
                return;
            }

            if (newStatus is < 0 or > (int)ConnectionStatus.Closed)
            {
                throw new InvalidOperationException("Cannot set status to invalid state");
            }

            Interlocked.Exchange(ref _connectionStatus, newStatus);
        }
    }

    private bool IsIdle => Status is ConnectionStatus.Idle;

    private bool IsConnected => Status is not (ConnectionStatus.Closed
        or ConnectionStatus.Connecting or ConnectionStatus.Broken);

    private long _inTransaction;
    public bool InTransaction
    {
        get => Interlocked.Read(ref _inTransaction) == 1;
        private set => Interlocked.Exchange(ref _inTransaction, value ? 1 : 0);
    }

    /// <summary>
    /// Open the underlining stream and initiate the connection with the database backend 
    /// </summary>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        CheckDisposed();
        using UserAction _ = StartUserAction(newStatus: ConnectionStatus.Connecting);
        try
        {
            await _asyncConnector.OpenAsync(
                    ConnectOptions.Host,
                    ConnectOptions.Port,
                    cancellationToken)
                .ConfigureAwait(false);
            await SendStartupMessage(ConnectOptions, cancellationToken).ConfigureAwait(false);
            await HandleAuthFlow(cancellationToken).ConfigureAwait(false);
            await WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            Status = ConnectionStatus.Idle;
            LastOpenTimestamp = DateTime.UtcNow;
        }
        catch
        {
            BreakConnection();
            throw;
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
        IAuthMessage authentication = await ReceiveAuthMessage(cancellationToken)
            .ConfigureAwait(false);
        switch (authentication)
        {
            case OkAuthMessage:
                // logged in!
                break;
            case ClearTextPasswordAuthMessage:
                ArgumentNullException.ThrowIfNull(ConnectOptions.Password);
                await SimplePasswordAuthFlow(ConnectOptions.Password, cancellationToken)
                    .ConfigureAwait(false);
                break;
            case MD5PasswordAuthMessage:
                throw new PgException(
                    "MD5 passwords are not supported by sqlx-cs-pg. They have been deprecated for removal by Postgres in version 18 so we will not support their usage");
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
        using UserAction _ = StartUserAction();
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            await SendQueryMessage("SELECT 1;", cancellationToken).ConfigureAwait(false);
            var result = await WaitForOrError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            if (result.IsRight)
            {
                BreakConnection();
                return false;
            }
        }
        catch (SqlxException)
        {
            BreakConnection();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Execute the query as either a simple query or extended query based upon the checks performed
    /// in <see cref="PgConnector.IsSimpleQuery"/>.
    /// </summary>
    /// <param name="query">Query to execute</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns></returns>
    /// <exception cref="PgException">
    /// If the supplied query is not a <see cref="PgExecutableQuery"/> or the connection is closed
    /// or broken.
    /// </exception>
    public Task<IAsyncResultSet<IPgDataRow>> ExecuteQuery(
        IPgExecutableQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return IsSimpleQuery(query)
#pragma warning disable CA2000
            ? SendSimpleQuery(query.Query, cancellationToken)
            : SendExtendedQuery(query, cancellationToken);
#pragma warning restore CA2000
    }

    public Task<IAsyncResultSet<IPgDataRow>> ExecuteQueryBatch(
        IPgQueryBatch queryBatch,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(queryBatch);
        return PipelineQueries(queryBatch, cancellationToken);
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
        TransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        ThrowIfNotOpen();
        using UserAction _ = StartUserAction();
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            switch (transactionCommand)
            {
                case TransactionCommand.Begin when InTransaction:
                    throw new UnexpectedTransactionState(false);
                case TransactionCommand.Commit
                    or TransactionCommand.Rollback when !InTransaction:
                    throw new UnexpectedTransactionState(true);
            }

            var sql = transactionCommand switch
            {
                TransactionCommand.Begin => BeginQuery,
                TransactionCommand.Commit => CommitQuery,
                TransactionCommand.Rollback => RollbackQuery,
                _ => throw new ArgumentOutOfRangeException(nameof(transactionCommand)),
            };
            await SendQueryMessage(sql, cancellationToken).ConfigureAwait(false);
            await WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            InTransaction = transactionCommand is TransactionCommand.Begin;
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
            if (transactionCommand is not TransactionCommand.Commit) throw;

            try
            {
                await SendQueryMessage(RollbackQuery, cancellationToken)
                    .ConfigureAwait(false);
                await WaitForOrError<ReadyForQueryMessage>(cancellationToken)
                    .ConfigureAwait(false);
            }
#pragma warning disable CA1031
            catch
#pragma warning restore CA1031
            {
                // ignored
            }

            throw;
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
            ReadyForQueryMessage message =
                await WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
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
        HandleReadyForQuery(readyForQuery.TransactionStatus);
    }

    /// <summary>
    /// Decrement <see cref="_pendingReadyForQuery"/> and inspect the supplied message
    /// </summary>
    /// <param name="transactionStatus">status update from server</param>
    private void HandleReadyForQuery(TransactionStatus transactionStatus)
    {
        if (--_pendingReadyForQuery < 0)
        {
            _logger.LogReceivedMoreReadyForQueryThanExpected();
            _pendingReadyForQuery = 0;
        }

        if (transactionStatus is TransactionStatus.FailedTransaction)
        {
            _logger.LogServerReportedFailedTransaction();
        }
    }

    internal async Task<Either<PgNotification, ErrorResponseMessage>> WaitForNotificationOrError(
        CancellationToken cancellationToken)
    {
        Status = ConnectionStatus.Fetching;
        while (true)
        {
            PgBackendMessageType backendMessageType = await ReceiveNextMessageType(cancellationToken)
                .ConfigureAwait(false);
            var size = await ReceiveNextMessageSize(cancellationToken)
                .ConfigureAwait(false);

            if (ApplyStandardMessageProcessing(
                    backendMessageType,
                    size,
                    handleNotification: false,
                    throwOnError: false))
            {
                continue;
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            switch (backendMessageType)
            {
                case PgBackendMessageType.NotificationResponse:
                    var notification = ReceiveMessage<PgNotification>(size);
                    Status = ConnectionStatus.Idle;
                    return Either.Left<PgNotification, ErrorResponseMessage>(notification);
                case PgBackendMessageType.ErrorResponse:
                    var error = ReceiveMessage<ErrorResponseMessage>(size);
                    Status = ConnectionStatus.Idle;
                    return Either.Right<PgNotification, ErrorResponseMessage>(error);
                default:
                    AdvanceReadBuffer(size);
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessageType);
                    break;
            }
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
    internal async Task<T> WaitForOrThrowError<T>(CancellationToken cancellationToken)
        where T : IPgBackendMessage, IPgBackendMessageDecoder<T>
    {
        var result = await WaitForOrError<T>(cancellationToken)
            .ConfigureAwait(false);
        return result.IsLeft ? result.Left : throw new PgException(result.Right);
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
        where T : IPgBackendMessage, IPgBackendMessageDecoder<T>
    {
        while (true)
        {
            PgBackendMessageType backendMessageType =
                await ReceiveNextMessageType(cancellationToken)
                    .ConfigureAwait(false);
            var size = await ReceiveNextMessageSize(cancellationToken).ConfigureAwait(false);

            if (ApplyStandardMessageProcessing(backendMessageType, size, throwOnError: false))
            {
                continue;
            }

            if (backendMessageType is PgBackendMessageType.ErrorResponse)
            {
                var errorResponse = ReceiveMessage<ErrorResponseMessage>(size);
                return Either.Right<T, ErrorResponseMessage>(errorResponse);
            }

            if (backendMessageType == T.MessageType)
            {
                var result = ReceiveMessage<T>(size);
                return Either.Left<T, ErrorResponseMessage>(result);
            }
            
            AdvanceReadBuffer(size);
            _logger.LogIgnoreUnexpectedMessage(
                SqlxConfig.DetailedLoggingLevel,
                backendMessageType);
        }
    }

    /// <summary>
    /// Handle standard messages and return null when the message was handled. This generally
    /// applies to asynchronous messages that should be handled when sent out of order from the
    /// database backend.
    /// </summary>
    /// <param name="message">Message type to optionally process</param>
    /// <param name="size">
    /// Size of the message, used to consume contents of the message if needed
    /// </param>
    /// <param name="throwOnError">True if <see cref="ErrorResponseMessage"/> should throw</param>
    /// <param name="handleNotification">
    /// True if <see cref="PgNotification"/>s should be sent to <see cref="OnNotification"/>
    /// </param>
    /// <returns>True if the message was processed as a standard async message</returns>
    /// <exception cref="PgException">
    /// If <paramref name="throwOnError"/> is true and the message was an error response
    /// </exception>
    internal bool ApplyStandardMessageProcessing(
        PgBackendMessageType message,
        int size,
        bool throwOnError = true,
        bool handleNotification = true)
    {
        switch (message)
        {
            case PgBackendMessageType.NoticeResponse:
                var noticeResponse = ReceiveMessage<NoticeResponseMessage>(size);
                OnNotice(noticeResponse);
                return true;
            case PgBackendMessageType.NotificationResponse:
                if (!handleNotification) return false;
                var notification = ReceiveMessage<PgNotification>(size);
                OnNotification(notification);
                return true;
            case PgBackendMessageType.BackendDataKey:
                var backendDataKey = ReceiveMessage<BackendDataKeyMessage>(size);
                OnBackendDataKey(backendDataKey);
                return true;
            case PgBackendMessageType.ParameterStatus:
                var parameterStatus = ReceiveMessage<ParameterStatusMessage>(size);
                OnParameterStatus(parameterStatus);
                return true;
            case PgBackendMessageType.NegotiateProtocolVersion:
                var negotiateProtocolVersion =
                    ReceiveMessage<NegotiateProtocolVersionMessage>(size);
                OnNegotiateProtocolVersion(negotiateProtocolVersion);
                return true;
            case PgBackendMessageType.ErrorResponse:
                var errorResponse = ReceiveMessage<ErrorResponseMessage>(size);
                if (errorResponse.InformationResponse.Code.IsCriticalConnectionError)
                {
                    BreakConnection();
                }
                return throwOnError ? throw new PgException(errorResponse) : false;
            default:
                return false;
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
    private void OnNotification(PgNotification notification)
    {
        _logger.LogNotification(SqlxConfig.DetailedLoggingLevel, notification);
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
    /// Write all content in <see cref="Writer"/> to the <see cref="_asyncConnector"/> and reset
    /// the <see cref="Writer"/> for future writes.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private async ValueTask FlushStream(CancellationToken cancellationToken)
    {
        FlushResult result = await Writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsCanceled)
        {
            throw new OperationCanceledException();
        }
    }

    internal readonly struct UserAction : IDisposable
    {
        private readonly PgConnector _connector;
        public UserAction(PgConnector connector) => _connector = connector;
        public void Dispose() => _connector.EndUserAction();
    }

    private UserAction StartUserAction(ConnectionStatus newStatus = ConnectionStatus.Executing)
    {
        ConnectionStatus currentStatus = Status;
        switch (currentStatus)
        {
            case ConnectionStatus.Idle:
            case ConnectionStatus.Closed when newStatus is ConnectionStatus.Connecting:
                break;
            case ConnectionStatus.Broken:
            case ConnectionStatus.Closed:
                throw new InvalidOperationException("The connection is not open");
            case ConnectionStatus.Connecting:
            case ConnectionStatus.Executing:
            case ConnectionStatus.Fetching:
                throw new InvalidOperationException($"Action against this connection is already in progress. Status = {currentStatus}");
            default:
#pragma warning disable CA2208
                throw new ArgumentOutOfRangeException(nameof(Status), "Invalid status flag");
#pragma warning restore CA2208
        }

        Status = newStatus;
        return new UserAction(this);
    }

    internal void EndUserAction()
    {
        if (IsIdle || !IsConnected)
        {
            return;
        }
        
        Status = ConnectionStatus.Idle;
    }

    internal void EndInProgressRequests()
    {
        _currentResultSet?.Dispose();
    }

    private void BreakConnection()
    {
        Status = ConnectionStatus.Broken;
        Dispose();
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        // If we can send messages to the server, sent the terminate command before closing
        if (Status is ConnectionStatus.Idle)
        {
            try
            {
                SendTerminate().AsTask().GetAwaiter().GetResult();
            }
#pragma warning disable CA1031
            catch (Exception e)
#pragma warning restore CA1031
            {
                _logger.LogErrorWhileClosingConnector(e);
            }
        }

        _currentResultSet?.Dispose();
        _asyncConnector.Dispose();
    }
}
