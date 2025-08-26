using Sqlx.Core.Connection;
using Sqlx.Core.Query;

namespace Sqlx.Core.Pool;

public interface IConnectionPool : IAsyncDisposable, IQueryExecutor
{
    public Task<IConnection> Acquire(CancellationToken cancellationToken = default);
    
    public IExecutableQuery CreateQuery(string sql);

    public IQueryBatch CreateQueryBatch();
}
