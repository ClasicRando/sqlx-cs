using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Cache;
using Sqlx.Core.Connection;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Connection;

/// <summary>
/// <see cref="IConnection"/> implementation for a Postgresql database connection. Beyond default
/// connection implementations, other Postgresql specific functionality is implemented such as
/// <c>LISTEN/NOTIFY</c> and the <c>COPY</c> protocol.
/// </summary>
public sealed partial class PgConnection : IConnection
{
    private const string BeginQuery = "BEGIN;";
    private const string CommitQuery = "COMMIT;";
    private const string RollbackQuery = "ROLLBACK;";
    
    private readonly PgStream _pgStream;
    internal PgConnectionPool? Pool;
    private readonly ILogger _logger;
    private long _inTransaction;
    private int _pendingReadyForQuery;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    internal PgConnection(PgStream pgStream, PgConnectionPool pool)
    {
        _pgStream = pgStream;
        Pool = pool;
        _logger = pgStream.ConnectOptions.LoggerFactory.CreateLogger<PgConnection>();
        _statementCache = new LruCache<string, PgPreparedStatement>(
            _pgStream.ConnectOptions.StatementCacheCapacity);
    }

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Closed;
    
    public bool InTransaction
    {
        get => Interlocked.Read(ref _inTransaction) == 1;
        private set => Interlocked.Exchange(ref _inTransaction, value ? 1 : 0);
    }

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (Status == ConnectionStatus.Idle)
            {
                return;
            }
            
            Status = ConnectionStatus.Connecting;
            await _pgStream.OpenAsync(cancellationToken).ConfigureAwait(false);
            Status = ConnectionStatus.Idle;
        }
        catch (SqlxException)
        {
            Status = ConnectionStatus.Broken;
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private enum TransactionCommand
    {
        Begin = 0,
        Commit = 1,
        Rollback = 2,
    }

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommand(TransactionCommand.Begin, cancellationToken);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommand(TransactionCommand.Commit, cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommand(TransactionCommand.Rollback, cancellationToken);
    }

    public async Task<bool> IsValidAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfBrokenOrClosed();
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            await _pgStream.SendQueryMessage("SELECT 1;", cancellationToken).ConfigureAwait(false);
            var result = await _pgStream.WaitForOrError<ReadyForQueryMessage>(cancellationToken)
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

    public IExecutableQuery CreateQuery(string query)
    {
        
        return new PgExecutableQuery(query, this);
    }

    public IQueryBatch CreateQueryBatch()
    {
        throw new NotImplementedException();
    }

    public async Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteQuery(
        IQuery query,
        CancellationToken cancellationToken)
    {
        PgExecutableQuery executableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        await ConnectIfClosed(cancellationToken);
        var results = executableQuery.ParameterBuffer.ParameterCount == 0
            ? SendSimpleQuery(executableQuery.Query, cancellationToken)
            : SendExtendedQuery(executableQuery.Query, executableQuery.ParameterBuffer, cancellationToken);
        return results;
    }

    public Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteQueryBatch(
        IQueryBatch query,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async ValueTask ConnectIfClosed(CancellationToken cancellationToken)
    {
        if (Status is not ConnectionStatus.Closed)
        {
            return;
        }

        await OpenAsync(cancellationToken).ConfigureAwait(false);
    }

    internal ValueTask ReleaseResources(CancellationToken cancellationToken)
    {
        Status = ConnectionStatus.Closed;
        return _pgStream.CloseAsync(cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (Pool is not null
                && await Pool.GiveBack(this, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await ReleaseResources(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            Status = ConnectionStatus.Broken;
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <exception cref="PgException">
    /// If the <see cref="Status"/> value is <see cref="ConnectionStatus.Broken"/> or
    /// <see cref="ConnectionStatus.Closed"/>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfBrokenOrClosed()
    {
        if (Status is ConnectionStatus.Broken or ConnectionStatus.Closed)
        {
            throw new PgException(
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
    private async Task ExecuteTransactionCommand(
        TransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        ThrowIfBrokenOrClosed();
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            switch (transactionCommand)
            {
                case TransactionCommand.Begin when InTransaction:
                    throw new UnexpectedTransactionState(false);
                case TransactionCommand.Commit or TransactionCommand.Rollback when !InTransaction:
                    throw new UnexpectedTransactionState(true);
            }

            var sql = transactionCommand switch
            {
                TransactionCommand.Begin => BeginQuery,
                TransactionCommand.Commit => CommitQuery,
                TransactionCommand.Rollback => RollbackQuery,
                _ => throw PgException.EnumOutOfRange(transactionCommand),
            };
            await _pgStream.SendQueryMessage(sql, cancellationToken).ConfigureAwait(false);
            await _pgStream.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken).ConfigureAwait(false);
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
                await _pgStream.SendQueryMessage(RollbackQuery, cancellationToken)
                    .ConfigureAwait(false);
                await _pgStream.WaitForOrError<ReadyForQueryMessage>(cancellationToken)
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
            ReadyForQueryMessage message = await _pgStream.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
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

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
    }
}
