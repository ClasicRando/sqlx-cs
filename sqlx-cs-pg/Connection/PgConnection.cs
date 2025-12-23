using Sqlx.Core;
using Sqlx.Core.Connection;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Connection;

/// <summary>
/// <see cref="IPgConnection"/> implementation for a Postgresql database connection. Beyond default
/// connection implementations, other Postgresql specific functionality is implemented such as
/// <c>LISTEN/NOTIFY</c> and the <c>COPY</c> protocol.
/// </summary>
public sealed class PgConnection : AbstractConnection<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>, IPgConnection
{
    private bool _disposed;
    private PgStream? _pgStream;
    private readonly PgConnectionPool _pool;

    internal PgConnection(PgConnectionPool pool)
    {
        _pool = pool;
    }

    public override ConnectionStatus Status => _pgStream?.Status ?? ConnectionStatus.Closed;
    
    public override bool InTransaction => _pgStream?.InTransaction ?? false;

    public override async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        CheckClosed();
        PgStream stream = await _pool.AcquireStream(cancellationToken).ConfigureAwait(false);
        _pgStream = stream;
    }

    public override IPgExecutableQuery CreateQuery(string query)
    {
        return new PgExecutableQuery(query, this);
    }

    public override IPgQueryBatch CreateQueryBatch()
    {
        return new PgQueryBatch(this);
    }

    public override async Task<IAsyncEnumerable<Either<IPgDataRow, QueryResult>>> ExecuteQuery(
        IPgExecutableQuery query,
        CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken);
        return _pgStream!.ExecuteQuery(query, cancellationToken);
    }

    public override async Task<IAsyncEnumerable<Either<IPgDataRow, QueryResult>>> ExecuteQueryBatch(
        IPgQueryBatch query,
        CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken);
        return _pgStream!.ExecuteQueryBatch(query, cancellationToken);
    }

    public override Task CloseAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();
        try
        {
            if (Status is ConnectionStatus.Closed or ConnectionStatus.Broken)
            {
                return Task.CompletedTask;
            }

            _pool.Return(_pgStream!);
        }
        finally
        {
            _pgStream = null;
        }
        return Task.CompletedTask;
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
    protected override Task ExecuteTransactionCommand(
        TransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        CheckDisposed();
        return _pgStream!.ExecuteTransactionCommand(transactionCommand, cancellationToken);
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

    private void CheckClosed()
    {
        CheckDisposed();

        if (Status is not (ConnectionStatus.Closed or ConnectionStatus.Broken))
        {
            throw new InvalidOperationException("Connection is already open");
        }
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        _disposed = true;
    }
}
