using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Cache;
using Sqlx.Core.Connection;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Connection;

public sealed partial class PgConnection : IConnection, IQueryExecutor
{
    private readonly PgStream _pgStream;
    internal PgConnectionPool? Pool;
    private readonly ILogger _logger;
    private long _inTransaction;
    private int _pendingReadyForQuery;
    private readonly SemaphoreSlim _semaphore = new(0, 1);

    internal PgConnection(PgStream pgStream, PgConnectionPool pool)
    {
        _pgStream = pgStream;
        Pool = pool;
        _logger = pgStream.ConnectOptions.LoggerFactory.CreateLogger<PgConnection>();
        _statementCache = new LruCache<string, PgPreparedStatement>(
            _pgStream.ConnectOptions.StatementCacheCapacity);
    }

    public bool IsConnected => _pgStream.IsConnected;

    public bool InTransaction
    {
        get => Interlocked.Read(ref _inTransaction) == 1;
        private set => Interlocked.Exchange(ref _inTransaction, value ? 1 : 0);
    }

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        await _pgStream.OpenAsync(cancellationToken).ConfigureAwait(false);
    }

    private enum TransactionCommand
    {
        Begin = 0,
        Commit = 1,
        Rollback = 2,
    }

    public Task Begin(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommand(TransactionCommand.Begin, cancellationToken);
    }

    public Task Commit(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommand(TransactionCommand.Commit, cancellationToken);
    }

    public Task Rollback(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommand(TransactionCommand.Rollback, cancellationToken);
    }

    public async Task<bool> IsValid(CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            await _pgStream.SendMessage(new QueryMessage("SELECT 1;"), cancellationToken)
                .ConfigureAwait(false);
            var result = await _pgStream.WaitForOrError<ReadyForQueryMessage>(cancellationToken)
                .ConfigureAwait(false);
            if (result is { Right: not null })
            {
                return false;
            }
        }
        catch (SqlxException)
        {
            return false;
        }
        finally
        {
            _semaphore.Release();
        }

        return true;
    }

    public IExecutableQuery CreateQuery(string sql)
    {
        return new PgExecutableQuery(sql, this);
    }

    public IQueryBatch CreateQueryBatch()
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Either<IDataRow, QueryResult>> ExecuteQuery(
        IQuery query,
        CancellationToken cancellationToken)
    {
        PgExecutableQuery executableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return executableQuery.ParameterBuffer.ParameterCount == 0
            ? SendSimpleQuery(query.Query, cancellationToken)
            : SendExtendedQuery(query.Query, executableQuery.ParameterBuffer, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (Pool is not null && await Pool.GiveBack(this, cancellationToken))
        {
            return;
        }
        await _pgStream.CloseAsync(cancellationToken).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfNotConnected()
    {
        if (!IsConnected)
        {
            throw new PgException("Attempted to perform operation before opening connection");
        }
    }

    private async Task ExecuteTransactionCommand(
        TransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();
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
                TransactionCommand.Begin => "BEGIN;",
                TransactionCommand.Commit => "COMMIT;",
                TransactionCommand.Rollback => "ROLLBACK;",
                _ => throw SqlxException.EnumOutOfRange(transactionCommand),
            };
            await _pgStream.SendMessage(new QueryMessage(sql), cancellationToken).ConfigureAwait(false);
            await _pgStream.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken).ConfigureAwait(false);
            InTransaction = transactionCommand == 0;
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
            if (transactionCommand is TransactionCommand.Commit)
            {
                try
                {
                    await _pgStream.SendMessage(new QueryMessage("ROLLBACK;"), cancellationToken)
                        .ConfigureAwait(false);
                    await _pgStream.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }
            InTransaction = false;
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task WaitUntilReady(CancellationToken cancellationToken)
    {
        while (_pendingReadyForQuery > 0)
        {
            ReadyForQueryMessage message = await _pgStream.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken).ConfigureAwait(false);
            HandleReadyForQuery(message);
        }
    }

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
