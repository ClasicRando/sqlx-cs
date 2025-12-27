using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Pool;

namespace Sqlx.Postgres.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DatabaseFixture : IDisposable
{
    public PgConnectionPool BasicPool { get; }

    public PgConnectionPool SimpleQueryTextPool { get; }

    public PgConnectionPool QueryTimeoutPool { get; }

    public DatabaseFixture()
    {
        const string host = "localhost";
        const int port = 5432;
        var username = Environment.GetEnvironmentVariable("PG_USERNAME")
                       ?? throw new Exception("PG_USERNAME is not present");
        var password = Environment.GetEnvironmentVariable("PG_PASSWORD")
                       ?? throw new Exception("PG_PASSWORD is not present");
        var database = Environment.GetEnvironmentVariable("PG_DATABASE")
                       ?? throw new Exception("PG_DATABASE is not present");
        var poolOptions = new PoolOptions();
        var options1 = new PgConnectOptions
        {
            Host = host,
            Port = port,
            Username = username,
            Database = database,
            Password = password,
        };
        BasicPool = new PgConnectionPool(options1, poolOptions);
        var options2 = new PgConnectOptions
        {
            Host = host,
            Port = port,
            Username = username,
            Database = database,
            Password = password,
            UseExtendedProtocolForSimpleQueries = false,
        };
        SimpleQueryTextPool = new PgConnectionPool(options2, poolOptions);
        var options3 = new PgConnectOptions
        {
            Host = host,
            Port = port,
            Username = username,
            Database = database,
            Password = password,
            QueryTimeout = TimeSpan.FromSeconds(1),
        };
        QueryTimeoutPool = new PgConnectionPool(options3, poolOptions);
    }

    public void Dispose()
    {
        BasicPool.DisposeAsync().AsTask().GetAwaiter().GetResult();
        SimpleQueryTextPool.DisposeAsync().AsTask().GetAwaiter().GetResult();
        QueryTimeoutPool.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
