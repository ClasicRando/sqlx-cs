using Sqlx.Core.Query;
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
            await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
            ICopyTo copyStatement = new CopyTableToCsv
            {
                SchemaName = "public",
                TableName = "copy_out_test",
            };

            await connection.CopyOutAsync(copyStatement, tempPath, FileMode.Create, ct);

            var rawCsvData = await File.ReadAllLinesAsync(tempPath, ct);
            var csvData = rawCsvData
                .Select(csv =>
                {
                    var split = csv.Split(',', 2);
                    return new CopyRow { Id = int.Parse(split[0]), TextField = split[1] };
                })
                .OrderBy(row => row.Id);
            var expectedCsvData = Enumerable.Range(1, CopyRowCount)
                .Select(rowIndex => new CopyRow { Id = rowIndex, TextField = $"{rowIndex} Value" });
            foreach ((CopyRow actual, CopyRow expected) in csvData.Zip(expectedCsvData))
            {
                await Assert.That(actual).IsEqualTo(expected);
            }
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Test]
    public async Task CopyOutRowsAsync_Should_CopyRows_When_CopyTable(CancellationToken ct)
    {
        var copyStatement = new CopyTableToBinary
        {
            SchemaName = "public",
            TableName = "copy_out_test",
        };
        
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        var rows = await connection.CopyOutRowsAsync<CopyRow>(copyStatement, ct)
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
                Enumerable.Range(1, CopyRowCount)
                    .Select(rowIndex => $"{rowIndex},{rowIndex} Value"),
                ct);
            await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
            await using IPgExecutableQuery
                query = connection.CreateQuery("TRUNCATE public.copy_out_test");
            await query.ExecuteNonQueryAsync(ct);
            ICopyFrom copyStatement = new CopyTableFromCsv
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
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        var copyStatement = new CopyTableFromBinary
        {
            SchemaName = "public",
            TableName = "copy_out_test",
        };
        await using IPgExecutableQuery
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

    private static async Task VerifyCopyIn(
        IPgConnection connection,
        QueryResult result,
        CancellationToken ct)
    {
        await Assert.That(result).Member(r => r.RowsAffected, l => l.IsEqualTo(CopyRowCount));
        await Assert.That(result).Member(r => r.Message, l => l.IsEqualTo($"COPY {CopyRowCount}"));

        await using IPgExecutableQuery query1 =
            connection.CreateQuery("SELECT COUNT(*) FROM public.copy_out_test");
        var tableCount = await query1.ExecuteScalar<int>(ct);
        await Assert.That(tableCount).IsEqualTo(CopyRowCount);

        await using IPgExecutableQuery query2 = connection.CreateQuery(
            $"""
             SELECT COUNT(*)
             FROM public.copy_out_test ct
             FULL JOIN generate_series(1, {CopyRowCount}) t ON t.t = ct.id
             WHERE
                 ct.id IS NULL
                 OR t.t IS NULL
                 OR ct.text_field != ct.id || ' Value'
             """);
        var invalidRow = await query2.ExecuteScalarPg<int, PgInt>(ct);
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
                Id = dataRow.GetField<int>("id"),
                TextField = dataRow.GetField<string>("text_field"),
            };
        }

        public static short ColumnCount => 2;

        public void BindMany(IPgBindable bindable)
        {
            bindable.Bind(Id);
            bindable.Bind(TextField);
        }
    }
}
