using Sqlx.Core.Connection;

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
    public static async Task<IConnection> Begin(
        this IConnectionPool connectionPool,
        CancellationToken cancellationToken = default)
    {
        IConnection? connection = null;
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

    /// <summary>
    /// Create a connection from the pool and attempt to unwrap that connection as
    /// <typeparamref name="TConnection"/>. Equivalent to:
    /// <code>
    /// IConnection connection = connectionPool.CreateConnection();
    /// return connection.Unwrap&lt;TConnection&gt;();
    /// </code>
    /// </summary>
    /// <param name="connectionPool">connection pool to fetch a connection from</param>
    /// <typeparam name="TConnection">desired output connection type</typeparam>
    /// <returns>a connection from the pool as the desired type</returns>
    public static TConnection CreateConnectionAs<TConnection>(this IConnectionPool connectionPool)
        where TConnection : IConnection
    {
        return connectionPool.CreateConnection().Unwrap<TConnection>();
    }
}
