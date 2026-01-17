using Microsoft.Extensions.Logging;
using Sqlx.Core.Connector;
using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Notify;
using PgConnector = Sqlx.Postgres.Connector.PgConnector;

namespace Sqlx.Postgres.Pool;

internal sealed partial class PgConnectionPool
    : AbstractConnectionPool<PgConnector, PgConnectionPool>, IPgConnectionPool
{
    public PgConnectOptions ConnectOptions { get; }

    internal PgConnectionPool(PgConnectOptions options, PoolOptions poolOptions) : base(
        poolOptions,
        options.ConnectTimeout,
        options.LoggerFactory.CreateLogger<PgConnectionPool>())
    {
        ConnectOptions = options;
    }

    protected override PgConnector CreateNewConnection()
    {
        return new PgConnector(new AsyncConnector(), ConnectOptions);
    }

    public IPgConnection CreateConnection()
    {
        return new PgConnection(this);
    }

    public IPgListener CreateListener()
    {
        return new PgListener(this);
    }
}
