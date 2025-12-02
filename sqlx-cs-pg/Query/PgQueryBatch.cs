using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Postgres.Query;

/// <summary>
/// <see cref="IQueryBatch"/> implementation for Postgres. <see cref="IBindable"/> instances returned
/// are always <see cref="PgExecutableQuery"/> and the queries are executed using the
/// <see cref="IQueryExecutor"/> supplied to the constructor.
/// </summary>
public sealed class PgQueryBatch(IQueryExecutor queryExecutor) : IQueryBatch
{
    private bool _disposed;
    private IQueryExecutor? _queryExecutor = queryExecutor;
    private readonly List<PgExecutableQuery> _queries = [];
    
    public bool WrapBatchInTransaction { get; set; }

    internal IEnumerable<PgExecutableQuery> Queries => _queries;
    
    public IBindable CreateQuery(string sql)
    {
        CheckDisposed();
        var query = new PgExecutableQuery(sql, _queryExecutor!);
        _queries.Add(query);
        return query;
    }

    public Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteBatch(CancellationToken cancellationToken)
    {
        CheckDisposed();
        return _queryExecutor!.ExecuteQueryBatch(this, cancellationToken);
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, typeof(PgQueryBatch));
    
    public void Dispose()
    {
        _disposed = true;
        _queryExecutor = null;
        foreach (PgExecutableQuery query in _queries)
        {
            query.Dispose();
        }
        _queries.Clear();
    }
}
