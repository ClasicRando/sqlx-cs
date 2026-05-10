using Sqlx.Core.Pool;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Generator.Type;
using Sqlx.Postgres.Pool;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace Sqlx.Postgres.Fixtures;

public sealed class DatabaseFixture : IAsyncInitializer, IAsyncDisposable
{
    private const string ContainerDatabase = "sqlx_cs_tests";
    private const string ContainerUsername = "sqlx_cs_user";
    private const string ContainerPassword = "sqlx_cs_password";
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase(ContainerDatabase)
        .WithUsername(ContainerUsername)
        .WithPassword(ContainerPassword)
        .Build();
    
    public IPgConnectionPool BasicPool { get; private set; } = null!;

    public IPgConnectionPool SimpleQueryTextPool { get; private set; } = null!;

    public IPgConnectionPool QueryTimeoutPool { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        const string host = "localhost";
        await _container.StartAsync();
        
        var port = _container.GetMappedPublicPort(5432);
        var poolOptions = new PoolOptions();
        var options1 = new PgConnectOptions
        {
            Host = host,
            Port = port,
            Username = ContainerUsername,
            Database = ContainerDatabase,
            Password = ContainerPassword,
        };
        BasicPool = new PgConnectionPool(options1, poolOptions);
        var options2 = new PgConnectOptions
        {
            Host = host,
            Port = port,
            Username = ContainerUsername,
            Database = ContainerDatabase,
            Password = ContainerPassword,
            UseExtendedProtocolForSimpleQueries = false,
        };
        SimpleQueryTextPool = new PgConnectionPool(options2, poolOptions);
        var options3 = new PgConnectOptions
        {
            Host = host,
            Port = port,
            Username = ContainerUsername,
            Database = ContainerDatabase,
            Password = ContainerPassword,
            QueryTimeout = TimeSpan.FromSeconds(1),
        };
        QueryTimeoutPool = new PgConnectionPool(options3, poolOptions);
        
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
