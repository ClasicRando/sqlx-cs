using Sqlx.Core.Connection;

namespace Sqlx.Core.Pool;

public static class ConnectionPoolExtensions
{
    public static async Task<IConnection> Begin(
        this IConnectionPool connectionPool,
        CancellationToken cancellationToken = default)
    {
        IConnection? connection = null;
        try
        {
            connection = await connectionPool.Acquire(cancellationToken).ConfigureAwait(false);
            await connection.Begin(cancellationToken).ConfigureAwait(false);
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
