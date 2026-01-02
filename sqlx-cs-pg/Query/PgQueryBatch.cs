using Sqlx.Core;
using Sqlx.Core.Result;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Query;

/// <summary>
/// <see cref="IPgQueryBatch"/> implementation for Postgres. <see cref="IPgBindable"/> instances
/// returned are always <see cref="PgExecutableQuery"/> and the queries are executed using the
/// <see cref="IPgQueryExecutor"/> supplied to the constructor.
/// </summary>
public sealed class PgQueryBatch(IPgQueryExecutor queryExecutor) : IPgQueryBatch
{
    private bool _disposed;
    private IPgQueryExecutor? _queryExecutor = queryExecutor;
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

    public IAsyncEnumerable<Either<IPgDataRow, QueryResult>> ExecuteBatch(CancellationToken cancellationToken)
    {
        CheckDisposed();
        return _queryExecutor!.ExecuteQueryBatchAsync(this, cancellationToken);
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, typeof(PgQueryBatch));
    
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
}
