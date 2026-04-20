using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Connector;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Query;

/// <summary>
/// <see cref="IPgQueryBatch"/> implementation for Postgres. <see cref="IPgBindable"/> instances
/// returned are always <see cref="PgExecutableQuery"/> and the queries are executed using the
/// <see cref="PgConnector"/> supplied to the constructor.
/// </summary>
public sealed class PgQueryBatch(PgConnection queryExecutor) : IPgQueryBatch
{
    private bool _disposed;
#pragma warning disable CA2213
    private PgConnection? _queryExecutor = queryExecutor;
#pragma warning restore CA2213
    private readonly List<PgExecutableQuery> _queries = [];

    public bool WrapBatchInTransaction { get; set; }

    internal IReadOnlyList<PgExecutableQuery> Queries => _queries;

    public IPgBindable CreateQuery(string sql)
    {
        CheckDisposed();
        var query = new PgExecutableQuery(sql, _queryExecutor!);
        _queries.Add(query);
        return query;
    }

    public Task<IAsyncResultSet<IPgDataRow>> ExecuteBatchAsync(CancellationToken cancellationToken)
    {
        CheckDisposed();
        return _queryExecutor!.ExecuteQueryBatchAsync(this, cancellationToken);
    }

    private void CheckDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, typeof(PgQueryBatch));

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _queryExecutor = null;
        foreach (PgExecutableQuery query in _queries)
        {
            query.Dispose();
        }

        _queries.Clear();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
