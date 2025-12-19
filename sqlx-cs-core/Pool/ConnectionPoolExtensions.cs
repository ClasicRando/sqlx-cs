using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Core.Pool;

public static class ConnectionPoolExtensions
{
    /// <summary>
    /// Acquire a connection from the pool and immediately start a new transaction against the
    /// connection before returning that rented connection. If starting a transaction fails, the
    /// underlining connection is returned to the pool.
    /// </summary>
    /// <param name="connectionPool">connection pool to begin a transaction against</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <returns>a rented connection from the pool that is already within a transaction</returns>
    public static async Task<TConnection> Begin<TConnection, TBindable, TQuery, TQueryBatch, TDataRow>(
        this IConnectionPool<TConnection, TBindable, TQuery, TQueryBatch, TDataRow> connectionPool,
        CancellationToken cancellationToken = default)
        where TConnection : class, IConnection<TQuery, TBindable, TQueryBatch, TDataRow>
        where TBindable : IBindable
        where TQuery : IExecutableQuery<TDataRow>
        where TQueryBatch : IQueryBatch<TBindable, TDataRow>
        where TDataRow : IDataRow
    {
        TConnection? connection = null;
        try
        {
            connection = connectionPool.CreateConnection();
            if (connection.Status is ConnectionStatus.Closed)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            await connection.BeginAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch
        {
            if (connection == null) throw;
            await connection.CloseAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
