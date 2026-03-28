using System.Runtime.CompilerServices;
using Sqlx.Core.Connection;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;
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

    public async Task CopyOutAsync(
        ICopyTo copyOutStatement,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(copyOutStatement);
        ArgumentNullException.ThrowIfNull(stream);
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await _pgConnector!.CopyOut(copyOutStatement, stream, cancellationToken)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<TRow> CopyOutRowsAsync<TRow>(
        CopyTableToBinary copyOutStatement,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRow : IFromRow<IPgDataRow, TRow>
    {
        ArgumentNullException.ThrowIfNull(copyOutStatement);
        CheckDisposed();
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);

        var columns = await QueryTableMetadataAsync(copyOutStatement, cancellationToken)
            .ConfigureAwait(false);
        var statementMetadata = new PgStatementMetadata(columns);
        var rows = _pgConnector!
            .CopyOut<TRow>(copyOutStatement, statementMetadata, cancellationToken)
            .ConfigureAwait(false);
        await foreach (TRow row in rows)
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

    private async ValueTask<PgColumnMetadata[]> QueryTableMetadataAsync(
        CopyTableToBinary copyTable,
        CancellationToken cancellationToken)
    {
        IPgExecutableQuery query = CreateQuery(CopyTableMetadata.Query);
        // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
        await using var _ = query.ConfigureAwait(false);
        query.BindPg<string, PgString>(copyTable.TableName);
        query.BindPg<string, PgString>(copyTable.SchemaName);
        return await query.FetchAsync<CopyTableMetadata>(cancellationToken)
            .Select(m => m.GetColumnMetadata())
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private readonly record struct CopyTableMetadata(
        int TableOid,
        string ColumnName,
        short ColumnOrder,
        PgTypeInfo PgTypeInfo,
        short ColumnLength) : IFromRow<IPgDataRow, CopyTableMetadata>
    {
        public const string Query =
            """
            SELECT
                c.oid table_oid, attname AS column_name, attnum column_order, atttypid AS type_oid,
                attlen AS column_length
            FROM pg_attribute a
            JOIN pg_class c ON a.attrelid = c.oid
            JOIN pg_namespace n ON c.relnamespace = n.oid
            WHERE
                c.relname = $1
                AND n.nspname = $2
                AND a.attnum > 0
            """;

        public PgColumnMetadata GetColumnMetadata()
        {
            return new PgColumnMetadata(
                ColumnName,
                TableOid,
                ColumnOrder,
                PgTypeInfo,
                ColumnLength,
                0,
                PgFormatCode.Binary);
        }

        public static CopyTableMetadata FromRow(IPgDataRow dataRow)
        {
            return new CopyTableMetadata
            {
                TableOid = dataRow.GetPgNotNull<int, PgInt>("table_oid"),
                ColumnName = dataRow.GetPgNotNull<string, PgString>("column_name"),
                ColumnOrder = dataRow.GetPgNotNull<short, PgShort>("column_order"),
                PgTypeInfo = PgTypeInfo.FromOid(dataRow.GetPgNotNull<PgOid>("type_oid")),
                ColumnLength = dataRow.GetPgNotNull<short, PgShort>("column_length"),
            };
        }
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
