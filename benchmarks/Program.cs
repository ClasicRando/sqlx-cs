using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Npgsql;
using NpgsqlTypes;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Postgres;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Query;

namespace benchmarks;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}

[MemoryDiagnoser]
public class Benchmarks
{
    private static readonly string Username =
        Environment.GetEnvironmentVariable("BENCHMARK_USERNAME") ??
        throw new InvalidOperationException("Cannot find username");
    private static readonly string Database =
        Environment.GetEnvironmentVariable("BENCHMARK_DATABASE") ??
        throw new InvalidOperationException("Cannot find database");
    private static readonly string Password =
        Environment.GetEnvironmentVariable("BENCHMARK_PASSWORD") ??
        throw new InvalidOperationException("Cannot find password");

    private const string SingleRowQuery =
        """
        SELECT id, text_field, creation_date, last_change_date, counter
        FROM public.posts
        WHERE id = $1
        """;
    
    private const string MultiRowQuery =
        """
        SELECT id, text_field, creation_date, last_change_date, counter
        FROM public.posts
        WHERE id BETWEEN $1 AND $2
        """;
    
    private const string RowQuery =
        """
        SELECT id, text_field, creation_date, last_change_date, counter
        FROM public.posts
        """;
    
    private const int MultiConnectionCount = 10;

    private static NpgsqlDataSource _npgsqlDataSource = null!;
    private static IPgConnectionPool _sqlxPgConnectionPool = null!;


    [GlobalSetup]
    public void SetUp()
    {
        _npgsqlDataSource = new NpgsqlDataSourceBuilder
        {
            ConnectionStringBuilder =
            {
                Host = "localhost",
                Port = 5432,
                Username = Username,
                Database = Database,
                Password = Password,
            },
        }.Build();
        _sqlxPgConnectionPool = IPgConnectionPool.Create(
            connectOptions: new PgConnectOptions
            {
                Host = "localhost",
                Port = 5432,
                Username = Username,
                Database = Database,
                Password = Password,
            },
            poolOptions: new PoolOptions());
        _sqlxPgConnectionPool.ExecuteNonQueryAsync(
            """
            DROP TABLE IF EXISTS public.posts;
            CREATE TABLE public.posts (
                id int primary key generated always as identity, 
                text_field text not null, 
                creation_date timestamp not null,
                last_change_date timestamp not null,
                counter int
            );
            
            INSERT INTO public.posts(text_field, creation_date, last_change_date)
            SELECT REPEAT('x', 2000), current_timestamp, current_timestamp
            FROM generate_series(1, 5000) s
            """).GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void CleanUp()
    {
        _npgsqlDataSource.Dispose();
        _sqlxPgConnectionPool.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    
    private int _id;

    [IterationSetup]
    public void Init()
    {
        _id++;
        if (_id > 5000) _id = 1;
    }

    [Benchmark]
    public async Task<List<RowData>> SimpleQuerySingleRowNpgsql()
    {
        await using NpgsqlCommand command = _npgsqlDataSource.CreateCommand(SingleRowQuery);
        command.Parameters.AddWithValue(NpgsqlDbType.Integer, _id);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        return await CollectDataReaderRows(reader);
    }

    [Benchmark]
    public async Task<List<RowData>> SimpleQueryMultiRowNpgsql()
    {
        await using NpgsqlCommand command = _npgsqlDataSource.CreateCommand(MultiRowQuery);
        command.Parameters.AddWithValue(NpgsqlDbType.Integer, _id);
        command.Parameters.AddWithValue(NpgsqlDbType.Integer, _id + 10);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        return await CollectDataReaderRows(reader);
    }

    [Benchmark]
    public async Task<List<RowData>> SimpleQueryAllRowNpgsql()
    {
        await using NpgsqlCommand command = _npgsqlDataSource.CreateCommand(RowQuery);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        return await CollectDataReaderRows(reader);
    }

    [Benchmark]
    public async Task<List<RowData>[]> SimpleQueryMultiConnectionsAllRowNpgsql()
    {
        List<Task<List<RowData>>> tasks = [];
        for (var i = 0; i < MultiConnectionCount; i++)
        {
            tasks.Add(Task.Run(SimpleQueryAllRowNpgsql));
        }
        return await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task<List<RowData>> SimpleQuerySingleRowSqlx()
    {
        var param = new IdParam { Id = _id };
        return await _sqlxPgConnectionPool.FetchAllAsync<IdParam, RowData>(SingleRowQuery, param);
    }

    [Benchmark]
    public async Task<List<RowData>> SimpleQueryMultiRowSqlx()
    {
        var param = new IdPairParam { Id1 = _id, Id2 = _id + 10};
        return await _sqlxPgConnectionPool.FetchAllAsync<IdPairParam, RowData>(MultiRowQuery, param);
    }

    [Benchmark]
    public async Task<List<RowData>> SimpleQueryAllRowSqlx()
    {
        return await _sqlxPgConnectionPool.FetchAllAsync<RowData>(RowQuery);
    }

    [Benchmark]
    public async Task<List<RowData>[]> SimpleQueryMultiConnectionsAllRowSqlx()
    {
        List<Task<List<RowData>>> tasks = [];
        for (var i = 0; i < MultiConnectionCount; i++)
        {
            tasks.Add(Task.Run(SimpleQueryAllRowSqlx));
        }
        return await Task.WhenAll(tasks);
    }

    private static async Task<List<RowData>> CollectDataReaderRows(NpgsqlDataReader reader)
    {
        List<RowData> data = [];
        while (await reader.ReadAsync())
        {
            var counterIndex = reader.GetOrdinal("counter");
            var row = new RowData
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Text = reader.GetString(reader.GetOrdinal("text_field")),
                CreationDate = reader.GetDateTime(reader.GetOrdinal("creation_date")),
                LastChangeDate = reader.GetDateTime(reader.GetOrdinal("last_change_date")),
                Counter = reader.IsDBNull(counterIndex) ? null : reader.GetInt32(counterIndex),
            };
            data.Add(row);
        }

        return data;
    }
}

[FromRow(RenameAll = Rename.SnakeCase)]
public readonly partial struct RowData
{
    public required int Id { get; init; }
        
    [Sqlx.Postgres.Generator.PgName("text_field")]
    public required string Text { get; init; }
        
    public required DateTime CreationDate { get; init; }
        
    public required DateTime LastChangeDate { get; init; }
        
    public required int? Counter { get; init; }

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}, {nameof(CreationDate)}: {CreationDate}, {nameof(LastChangeDate)}: {LastChangeDate}, {nameof(Counter)}: {Counter}";
    }
}

public readonly struct IdParam : IBindMany<IPgBindable>
{
    public required int Id { get; init; }

    public void BindMany(IPgBindable bindable)
    {
        bindable.Bind(Id);
    }
}

public readonly struct IdPairParam : IBindMany<IPgBindable>
{
    public required int Id1 { get; init; }
    
    public required int Id2 { get; init; }

    public void BindMany(IPgBindable bindable)
    {
        bindable.Bind(Id1);
        bindable.Bind(Id2);
    }
}
