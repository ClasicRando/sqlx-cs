// See https://aka.ms/new-console-template for more information

using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Query;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Pool;
using Sqlx.Postgres.Result;

namespace examples;

public static class Postgres
{
    public static async Task Run()
    {
        var username = Environment.GetEnvironmentVariable("PG_EXAMPLE_USERNAME")
                       ?? throw new Exception("Could not find env variable");
        var password = Environment.GetEnvironmentVariable("PG_EXAMPLE_PASSWORD")
                       ?? throw new Exception("Could not find env variable");

        var options = new PgConnectOptions
        {
            Host = "localhost",
            Username = username,
            Database = Environment.GetEnvironmentVariable("PG_EXAMPLE_DATABASE"),
            Password = password,
        };
        PoolOptions poolOptions = new();

        await using var pool = IPgConnectionPool.Create(options, poolOptions);

        const string query = 
            """
            SELECT
                s.value AS id, REPEAT('x', 2000) AS text_field,
                current_timestamp AS creation_date, current_timestamp AS last_change_date,
                NULL AS counter
            FROM generate_series(1, $1) s(value);
            """;

        await foreach (Row row in pool.FetchAsync<Param, Row>(query, new Param { Count = 100 }))
        {
            Console.WriteLine(row);
        }
    }
}

[ToParam]
public readonly partial struct Param
{
    public required int Count { get; init; }
}

// [FromRow(RenameAll = Rename.SnakeCase)]
public readonly partial struct Row : IFromRow<IPgDataRow, Row>
{
    public required int Id { get; init; }

    [PgName("text_field")]
    public required string Text { get; init; }

    public required DateTime CreationDate { get; init; }

    public required DateTime LastChangeDate { get; init; }

    public required int? Counter { get; init; }

    public static Row FromRow(IPgDataRow dataRow)
    {
        return new Row
        {
            Id = dataRow.GetField<int>("id"),
            Text = dataRow.GetField<string>("text_field"),
            CreationDate = dataRow.GetField<DateTime>("creation_date"),
            LastChangeDate = dataRow.GetField<DateTime>("last_change_date"),
            Counter = dataRow.GetField<int?>("counter"),
        };
    }

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}, {nameof(CreationDate)}: {CreationDate}, {nameof(LastChangeDate)}: {LastChangeDate}, {nameof(Counter)}: {Counter}";
    }
}
