using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Core.Pool;

/// <summary>
/// Database connection pool type. Pools connections for reuse over the lifetime of an application
/// and automatically manages ejecting broken connections so they are not reused. Pools can also
/// be used to execute queries against so you don't have to worry about connection management to
/// query the database. Behind the scenes connection pools borrow a connection for the lifetime of
/// the query execution so make sure to pull all query results as soon as possible to ensure you are
/// not starving the connection pool.
/// </summary>
public interface IConnectionPool<out TConnection, out TBindable, TQuery, TQueryBatch, TDataRow> :
    IAsyncDisposable
    where TConnection : IConnection<TQuery, TBindable, TQueryBatch, TDataRow>
    where TBindable : IBindable
    where TQuery : IExecutableQuery<TDataRow>
    where TQueryBatch : IQueryBatch<TBindable, TDataRow>
    where TDataRow : IDataRow
{
    PoolOptions PoolOptions { get; }
    
    /// <summary>
    /// Create a connection for use in querying this pool's database. The created connection is
    /// closed until <see cref="IConnection{TQuery,TBindable,TQueryBatch,TDataRow}.OpenAsync"/> is
    /// called and this method will return immediately with the created connection. However, it will
    /// wait for an available physical connection to become available from the pool before
    /// <see cref="IConnection{TQuery,TBindable,TQueryBatch,TDataRow}.OpenAsync"/> returns. Make
    /// sure to close the connection instance to release any system resources held by the connection
    /// or to return it to the underlining pool.
    /// </summary>
    /// <returns>A new connection object</returns>
    TConnection CreateConnection();
}
