using Sqlx.Core.Connection;
using Sqlx.Core.Query;

namespace Sqlx.Core.Pool;

/// <summary>
/// Database connection pool type. Pools connections for reuse over the lifetime of an application
/// and automatically manages ejecting broken connections so they are not reused. Pools can also
/// be used to execute queries against so you don't have to worry about connection management to
/// query the database. Behind the scenes connection pools borrow a connection for the lifetime of
/// the query execution so make sure to pull all query results as soon as possible to ensure you are
/// not starving the connection pool.
/// </summary>
public interface IConnectionPool : IQueryExecutor, IAsyncDisposable
{
    /// <summary>
    /// Rent a connection for use in querying the database. Make sure to close the connection
    /// instance to return it to the pool for reuse (preferably using the
    /// <see cref="IAsyncDisposable"/> interface's language construct).
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IConnection> Acquire(CancellationToken cancellationToken = default);
}
