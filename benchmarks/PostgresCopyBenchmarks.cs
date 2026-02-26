using System.Globalization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CsvHelper;
using Npgsql;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Pool;

namespace benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PostgresCopyBenchmarks
{
    private const int RowCount = 50_000;
    private static readonly string Username =
        Environment.GetEnvironmentVariable("PG_BENCHMARK_USERNAME") ??
        throw new InvalidOperationException("Cannot find username");
    private static readonly string Database =
        Environment.GetEnvironmentVariable("PG_BENCHMARK_DATABASE") ??
        throw new InvalidOperationException("Cannot find database");
    private static readonly string Password =
        Environment.GetEnvironmentVariable("PG_BENCHMARK_PASSWORD") ??
        throw new InvalidOperationException("Cannot find password");

    private static NpgsqlDataSource _npgsqlDataSource = null!;
    private static IPgConnectionPool _sqlxPgConnectionPool = null!;
    private static string _tempCsvFileInput = string.Empty;

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
                Username = Username,
                Database = Database,
                Password = Password,
            },
            poolOptions: new PoolOptions());
        _sqlxPgConnectionPool.ExecuteNonQueryAsync(
            """
            DROP TABLE IF EXISTS public.copy_target;
            CREATE TABLE public.copy_target(
                id int primary key,
                text_field text not null,
                creation_date timestamp not null,
                last_change_date timestamp not null,
                counter int
            );
            """).GetAwaiter().GetResult();
        CreateCsvFile();
    }

    private static void CreateCsvFile()
    {
        _tempCsvFileInput = Path.Combine(Path.GetTempPath(), "copy-benchmark.csv");
        File.Delete(_tempCsvFileInput);
        using var stream = new FileStream(_tempCsvFileInput, FileMode.OpenOrCreate);
        using var writer = new StreamWriter(stream);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        for (var i = 1; i <= RowCount; i++)
        {
            DateTime timestamp = DateTime.UtcNow;
            csvWriter.WriteRecord(new RowData
            {
                Id = i,
                Text = i.ToString(),
                CreationDate = timestamp,
                LastChangeDate = timestamp,
                Counter = null,
            });
            csvWriter.NextRecord();
        }
    }

    [GlobalCleanup]
    public void CleanUp()
    {
        _npgsqlDataSource.Dispose();
        _sqlxPgConnectionPool.DisposeAsync().AsTask().GetAwaiter().GetResult();
        File.Delete(_tempCsvFileInput);
    }

    [IterationSetup]
    public void Init()
    {
        _sqlxPgConnectionPool.ExecuteNonQueryAsync("TRUNCATE TABLE public.copy_target");
    }

    [Benchmark(Description = "Npgsql", Baseline = true), BenchmarkCategory("CopyIn, CSV")]
    public async Task CopyInCsvNpgsql()
    {
        await using NpgsqlConnection connection = _npgsqlDataSource.CreateConnection();
        await connection.OpenAsync();
        await using NpgsqlCopyTextWriter writer = await connection.BeginTextImportAsync("COPY public.copy_target FROM STDIN WITH (FORMAT CSV);");
        await using var stream = new FileStream(_tempCsvFileInput, FileMode.Open);
        await stream.CopyToAsync(writer.BaseStream);
    }

    [Benchmark(Description = "Npgsql", Baseline = true), BenchmarkCategory("CopyIn, Binary")]
    public async Task CopyInBinaryNpgsql()
    {
        await using NpgsqlConnection connection = _npgsqlDataSource.CreateConnection();
        await connection.OpenAsync();
        await using NpgsqlBinaryImporter writer = await connection.BeginBinaryImportAsync("COPY public.copy_target FROM STDIN WITH (FORMAT binary);");
        foreach (RowData row in GetRows())
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(row.Id);
            await writer.WriteAsync(row.Text);
            await writer.WriteAsync(row.CreationDate);
            await writer.WriteAsync(row.LastChangeDate);
            await writer.WriteAsync(row.Counter);
        }

        await writer.CompleteAsync();
    }

    [Benchmark(Description = "sqlx-cs-pg"), BenchmarkCategory("CopyIn, CSV")]
    public async Task<QueryResult> CopyInCsvSqlx()
    {
        await using IPgConnection connection = _sqlxPgConnectionPool.CreateConnection();
        TableFromCsv copyStatement = new()
        {
            SchemaName = "public",
            TableName = "copy_target",
        };
        return await connection.CopyInAsync(copyStatement, _tempCsvFileInput);
    }

    [Benchmark(Description = "sqlx-cs-pg"), BenchmarkCategory("CopyIn, Binary")]
    public async Task<QueryResult> CopyInBinarySqlx()
    {
        await using IPgConnection connection = _sqlxPgConnectionPool.CreateConnection();
        TableFromBinary copyStatement = new()
        {
            SchemaName = "public",
            TableName = "copy_target",
        };
        return await connection.CopyInRowsAsync(copyStatement, GetRows().ToAsyncEnumerable());
    }
    
    private static IEnumerable<RowData> GetRows()
    {
        DateTime timestamp = DateTime.UtcNow;
        for (var i = 1; i <= RowCount; i++)
        {
            yield return new RowData
            {
                Id = i,
                Text = i.ToString(),
                CreationDate = timestamp,
                LastChangeDate = timestamp,
                Counter = null,
            };
        }
    }
}
