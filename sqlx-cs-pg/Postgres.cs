using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Pool;

namespace Sqlx.Postgres;

public static class Postgres
{
    public static IPgConnectionPool Pool(PgConnectOptions connectOptions, PoolOptions poolOptions)
    {
        return new PgConnectionPool(connectOptions, poolOptions);
    }
}
