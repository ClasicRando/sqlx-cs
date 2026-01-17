using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Sqlx.Core;
using Sqlx.Core.Connection;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using PgConnector = Sqlx.Postgres.Connector.PgConnector;

namespace Sqlx.Postgres.Connection;

/// <summary>
/// <see cref="IPgConnection"/> implementation for a Postgresql database connection. Beyond default
/// connection implementations, other Postgresql specific functionality is implemented such as
/// <c>LISTEN/NOTIFY</c> and the <c>COPY</c> protocol.
/// </summary>
public sealed class PgConnection :
    AbstractConnection<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>, IPgConnection
{
    private bool _disposed;
    private PgConnector? _pgConnector;
    private readonly PgConnectionPool _pool;

    internal PgConnection(PgConnectionPool pool)
    {
        _pool = pool;
    }

    public override ConnectionStatus Status => _pgConnector?.Status ?? ConnectionStatus.Closed;

    public override bool InTransaction => _pgConnector?.InTransaction ?? false;

    public override async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        CheckClosed();
        PgConnector connector = await _pool.AcquireStreamAsync(cancellationToken).ConfigureAwait(false);
        _pgConnector = connector;
    }

    public override IPgExecutableQuery CreateQuery(string query)
    {
        return new PgExecutableQuery(query, this);
    }

    public override IPgQueryBatch CreateQueryBatch()
    {
        return new PgQueryBatch(this);
    }

    public override async IAsyncEnumerable<Either<IPgDataRow, QueryResult>> ExecuteQueryAsync(
        IPgExecutableQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await foreach (var item in _pgConnector!.ExecuteQuery(query, cancellationToken)
                           .ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public override async IAsyncEnumerable<Either<IPgDataRow, QueryResult>> ExecuteQueryBatchAsync(
        IPgQueryBatch query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await foreach (var item in _pgConnector!.ExecuteQueryBatch(query, cancellationToken)
                           .ConfigureAwait(false))
        {
            yield return item;
        }
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

            _pool.Return(_pgConnector!);
        }
        finally
        {
            _pgConnector = null;
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<byte[]> CopyOutAsync(
        ICopyTo copyOutStatement,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(copyOutStatement);
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        var rows = _pgConnector!.CopyOut(copyOutStatement, cancellationToken);
        await foreach (var row in rows.ConfigureAwait(false))
        {
            yield return row;
        }
    }

    public async Task<QueryResult> CopyInAsync(
        ICopyFrom copyInStatement,
        PipeReader data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(copyInStatement);
        ArgumentNullException.ThrowIfNull(data);
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        return await _pgConnector!.CopyIn(copyInStatement, data, cancellationToken)
            .ConfigureAwait(false);
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
    protected override async Task ExecuteTransactionCommandAsync(
        TransactionCommand transactionCommand,
        CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await _pgConnector!.ExecuteTransactionCommand(transactionCommand, cancellationToken)
            .ConfigureAwait(false);
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

    internal async Task<PgPreparedStatement> GetOrPrepareStatement(
        string sql,
        CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await _pgConnector!.WaitUntilReady(cancellationToken).ConfigureAwait(false);
        return await _pgConnector!.GetOrPrepareStatement(sql, [], cancellationToken)
            .ConfigureAwait(false);
    }
}
