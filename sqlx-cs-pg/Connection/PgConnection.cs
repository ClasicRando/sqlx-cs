using System.Runtime.CompilerServices;
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

    private async ValueTask OpenAsync(CancellationToken cancellationToken = default)
    {
        CheckDisposed();

        if (Status is not (ConnectionStatus.Closed or ConnectionStatus.Broken))
        {
            throw new InvalidOperationException("Connection is already open");
        }

        PgConnector connector = await _pool.AcquireStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        _pgConnector = connector;
    }

    public override IPgExecutableQuery CreateQuery(string query)
    {
        CheckDisposed();
        return new PgExecutableQuery(query, this);
    }

    public override IPgQueryBatch CreateQueryBatch()
    {
        CheckDisposed();
        return new PgQueryBatch(this);
    }

    internal async Task<IAsyncResultSet<IPgDataRow>> ExecuteQueryAsync(
        IPgExecutableQuery query,
        CancellationToken cancellationToken)
    {
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        return await _pgConnector!.ExecuteQuery(query, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<IAsyncResultSet<IPgDataRow>> ExecuteQueryBatchAsync(
        IPgQueryBatch query,
        CancellationToken cancellationToken)
    {
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        return await _pgConnector!.ExecuteQueryBatch(query, cancellationToken)
            .ConfigureAwait(false);
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
        Stream data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(copyInStatement);
        ArgumentNullException.ThrowIfNull(data);
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        return await _pgConnector!.CopyIn(copyInStatement, data, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<QueryResult> CopyInRowsAsync<TCopyStatement, TCopyRow>(
        TCopyStatement copyInStatement,
        IAsyncEnumerable<TCopyRow> rows,
        CancellationToken cancellationToken = default)
        where TCopyStatement : ICopyFrom, ICopyBinary
        where TCopyRow : IPgBinaryCopyRow
    {
        ArgumentNullException.ThrowIfNull(copyInStatement);
        ArgumentNullException.ThrowIfNull(rows);
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        return await _pgConnector!.CopyIn(copyInStatement, rows, cancellationToken)
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

    private void Close()
    {
        CheckDisposed();
        try
        {
            if (Status is ConnectionStatus.Closed or ConnectionStatus.Broken)
            {
                return;
            }

            _pgConnector?.EndInProgressRequests();

            _pool.Return(_pgConnector!);
        }
        finally
        {
            _pgConnector = null;
        }
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    protected override ValueTask DisposeAsyncCore()
    {
        if (_disposed) return ValueTask.CompletedTask;
        Close();
        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) Close();
        _disposed = true;
    }
}
