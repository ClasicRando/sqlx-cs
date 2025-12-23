using Microsoft.Extensions.Logging;
using Sqlx.Core.Pool;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Pool;

public sealed partial class PgConnectionPool
    : AbstractConnectionPool<PgStream, PgConnectionPool>,
        IConnectionPool<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch, IPgDataRow>
{
    public PgConnectOptions ConnectOptions { get; }

    internal PgConnectionPool(PgConnectOptions options, PoolOptions poolOptions) : base(
        poolOptions,
        options.ConnectTimeout,
        options.LoggerFactory.CreateLogger<PgConnectionPool>())
    {
        ConnectOptions = options;
    }

    protected override PgStream CreateNewConnection()
    {
        return new PgStream(new AsyncStream(), ConnectOptions);
    }

    public IPgConnection CreateConnection()
    {
        return new PgConnection(this);
    }
}
