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
    /// Create a connection for use in querying this pool's database. The created connection is
    /// closed until <see cref="IConnection.OpenAsync"/> is called and this method will return
    /// immediately with the created connection. However, it will wait for an available physical
    /// connection to become available from the pool before <see cref="IConnection.OpenAsync"/>
    /// returns. Make sure to close the connection instance to return it's underlining physical
    /// connection to the pool for reuse (preferably using the <see cref="IAsyncDisposable"/>
    /// interface's language construct).
    /// </summary>
    /// <returns>A connection linked to this pool</returns>
    IConnection CreateConnection();
}
