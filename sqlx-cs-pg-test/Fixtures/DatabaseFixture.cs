using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Generator.Type;
using Sqlx.Postgres.Pool;
using TUnit.Core.Interfaces;

namespace Sqlx.Postgres.Fixtures;

public sealed class DatabaseFixture : IAsyncInitializer, IAsyncDisposable
{
    public IPgConnectionPool BasicPool { get; }

    public IPgConnectionPool SimpleQueryTextPool { get; }

    public IPgConnectionPool QueryTimeoutPool { get; }

    public DatabaseFixture()
    {
        const string host = "localhost";
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
            Username = username,
            Database = database,
            Password = password,
        };
        BasicPool = new PgConnectionPool(options1, poolOptions);
        var options2 = new PgConnectOptions
        {
            Host = host,
            Username = username,
            Database = database,
            Password = password,
            UseExtendedProtocolForSimpleQueries = false,
        };
        SimpleQueryTextPool = new PgConnectionPool(options2, poolOptions);
        var options3 = new PgConnectOptions
        {
            Host = host,
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
        await CreateEnumType();
        await CreateCopyTable();
    }

    private async Task CreateStoredProcedures()
    {
        await BasicPool.ExecuteNonQueryAsync(PgConnectionTest.CreateProceduresQuery);
    }

    private async Task CreateCompositeType()
    {
        await BasicPool.ExecuteNonQueryAsync(PgConnectionTest.CreateTypeQuery);
        await BasicPool.MapCompositeAsync<TestCompositeType>();
    }

    private async Task CreateEnumType()
    {
        await BasicPool.ExecuteNonQueryAsync(PgConnectionTest.CreateEnumQuery);
        await BasicPool.MapTestPgEnumAsync();
    }

    private async Task CreateCopyTable()
    {
        await BasicPool.ExecuteNonQueryAsync(PgConnectionTest.CreateCopyTables);
    }

    public async ValueTask DisposeAsync()
    {
        await BasicPool.DisposeAsync();
        await SimpleQueryTextPool.DisposeAsync();
        await QueryTimeoutPool.DisposeAsync();
    }
}
