using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;
using TUnit.Core.Interfaces;

namespace Sqlx.Postgres.Fixtures;

public sealed class DatabaseFixture : IAsyncInitializer, IAsyncDisposable
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

    public async Task InitializeAsync()
    {
        await CreateStoredProcedures();
        await CreateCompositeType();
        await CreateCopyTable();
    }

    private async Task CreateStoredProcedures()
    {
        using IPgConnection connection = BasicPool.CreateConnection();
        using IPgExecutableQuery setUp = connection.CreateQuery(PgConnectionTest.CreateProceduresQuery);
        await setUp.ExecuteNonQueryAsync();
    }
    
    private async Task CreateCompositeType()
    {
        using IPgConnection connection = BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(PgConnectionTest.CreateTypeQuery);
        await query.ExecuteNonQueryAsync();
        await BasicPool.MapCompositeAsync<TestCompositeType>();
    }

    private async Task CreateCopyTable()
    {
        using IPgConnection connection = BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(PgConnectionTest.CreateCopyTables);
        await query.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await BasicPool.DisposeAsync();
        await SimpleQueryTextPool.DisposeAsync();
        await QueryTimeoutPool.DisposeAsync();
    }
}
