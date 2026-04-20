using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;

namespace Sqlx.Postgres.Pool;

public static class PostgresPool
{
    extension(IPgConnectionPool)
    {
        public static IPgConnectionPool Create(
            PgConnectOptions connectOptions,
            PoolOptions poolOptions)
        {
            ArgumentNullException.ThrowIfNull(connectOptions);
            ArgumentNullException.ThrowIfNull(poolOptions);
            return new PgConnectionPool(connectOptions, poolOptions);
        }
    }
}
