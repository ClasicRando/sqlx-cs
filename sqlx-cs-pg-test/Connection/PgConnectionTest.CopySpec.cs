using Sqlx.Core.Result;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    private const int CopyRowCount = 1_000_000;
    
    [Test]
    public async Task CopyOutAsync_Should_CopyDataToFile_When_CopyTableAndFilePath(
        CancellationToken ct)
    {
        var tempPath = Path.Join(Path.GetTempPath(), "copy-out-file.csv");
        try
        {
            using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
            ICopyTo copyStatement = new TableToCsv
            {
                SchemaName = "public",
                TableName = "copy_out_test",
            };

            await connection.CopyOutAsync(copyStatement, tempPath, FileMode.OpenOrCreate, ct);

            var csvData = await File.ReadAllLinesAsync(tempPath, ct);
            var rowIndex = 0;
            foreach (var csvRow in csvData)
            {
                rowIndex++;
                await Assert.That(csvRow).IsEqualTo($"{rowIndex},{rowIndex} Value");
            }
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

     [Test]
     public async Task CopyOutRowsAsync_Should_CopyRows_When_CopyQuery(CancellationToken ct)
     {
         var copyStatement = new QueryToBinary
         {
             Query = $"""
                     SELECT t.t id, t.t || ' Value' text_field
                     FROM generate_series(1, {CopyRowCount}) t
                     """,
         };
         await CopyOutRowsAsyncTest(copyStatement, ct);
     }

     [Test]
     public async Task CopyOutRowsAsync_Should_CopyRows_When_CopyTable(CancellationToken ct)
     {
         var copyStatement = new TableToBinary
         {
             SchemaName = "public",
             TableName = "copy_out_test",
         };
         await CopyOutRowsAsyncTest(copyStatement, ct);
     }

    private async Task CopyOutRowsAsyncTest<T>(T copyStatement, CancellationToken ct)
        where T : ICopyTo, ICopyBinary
    {
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();

        var rows = await connection.CopyOutRowsAsync<T, CopyRow>(copyStatement, ct)
            .OrderBy(cr => cr.Id)
            .ToListAsync(ct);
        var rowIndex = 0;
        foreach (CopyRow actualRow in rows)
        {
            rowIndex++;
            var expectedRow = new CopyRow { Id = rowIndex, TextField = $"{rowIndex} Value" };
            await Assert.That(actualRow).IsEqualTo(expectedRow);
        }
    }

    [Test]
    [NotInParallel(Order = 1)]
    public async Task CopyInAsync_Should_CopyDataFromFile_When_CopyTableAndFilePath(
        CancellationToken ct)
    {
        var tempPath = Path.Join(Path.GetTempPath(), "copy-in-file.csv");
        try
        {
            await File.WriteAllLinesAsync(
                tempPath,
                Enumerable.Range(1, CopyRowCount).Select(rowIndex => $"{rowIndex},{rowIndex} Value"),
                ct);
            using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
            using IPgExecutableQuery
                query = connection.CreateQuery("TRUNCATE public.copy_out_test");
            await query.ExecuteNonQueryAsync(ct);
            ICopyFrom copyStatement = new TableFromCsv
            {
                SchemaName = "public",
                TableName = "copy_out_test",
            };

            QueryResult result = await connection.CopyInAsync(copyStatement, tempPath, ct);

            await VerifyCopyIn(connection, result, ct);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    [NotInParallel(Order = 2)]
    public async Task CopyInRowsAsync_Should_CopyDataFromRows_When_CopyTableAndRows(
        CancellationToken ct)
    {
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
        var copyStatement = new TableFromBinary
        {
            SchemaName = "public",
            TableName = "copy_out_test",
        };
        using IPgExecutableQuery
            query = connection.CreateQuery("TRUNCATE public.copy_out_test");
        await query.ExecuteNonQueryAsync(ct);

        QueryResult result = await connection.CopyInRowsAsync(
            copyStatement,
            AsyncEnumerable.Range(1, CopyRowCount)
                .Select(rowIndex => new CopyRow
                    { Id = rowIndex, TextField = $"{rowIndex} Value" }),
            ct);

        await VerifyCopyIn(connection, result, ct);
    }

    private static async Task VerifyCopyIn(IPgConnection connection, QueryResult result, CancellationToken ct)
    {
        await Assert.That(result).Member(r => r.RowsAffected, l => l.IsEqualTo(CopyRowCount));
        await Assert.That(result).Member(r => r.Message, l => l.IsEqualTo($"COPY {CopyRowCount}"));

        using IPgExecutableQuery query1 =
            connection.CreateQuery("SELECT COUNT(*) FROM public.copy_out_test");
        var tableCount = await query1.ExecuteScalar<int, PgInt>(ct);
        await Assert.That(tableCount).IsEqualTo(CopyRowCount);

        using IPgExecutableQuery query2 = connection.CreateQuery(
            $"""
            SELECT COUNT(*)
            FROM public.copy_out_test ct
            FULL JOIN generate_series(1, {CopyRowCount}) t ON t.t = ct.id
            WHERE
                ct.id IS NULL
                OR t.t IS NULL
                OR ct.text_field != ct.id || ' Value'
            """);
        var invalidRow = await query2.ExecuteScalar<int, PgInt>(ct);
        await Assert.That(invalidRow).IsEqualTo(0);
    }

    public readonly record struct CopyRow : IFromRow<IPgDataRow, CopyRow>, IPgBinaryCopyRow
    {
        public required int Id { get; init; }

        public required string TextField { get; init; }

        public static CopyRow FromRow(IPgDataRow dataRow)
        {
            return new CopyRow
            {
                Id = dataRow.GetIntNotNull("id"),
                TextField = dataRow.GetStringNotNull("text_field"),
            };
        }

        public static short ColumnCount => 2;

        public void BindValues(IPgBindable bindable)
        {
            bindable.Bind(Id);
            bindable.Bind(TextField);
        }
    }
}
