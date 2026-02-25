using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    private const string OutProcedureCallSimpleQuery =
        $"CALL public.{OutProcedureName}(null, null);";

    [Test]
    public async Task ExecuteQuery_Should_ReturnOneResultSet_When_SimpleQuery(CancellationToken ct)
    {
        const string simpleQuery =
            """
            SELECT s.s, 'Regular Query' t
            FROM generate_series(1, 10) s
            """;
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(simpleQuery);
        using var resultSet = await query.ExecuteAsync(ct);
        var results = await resultSet.CollectResults(row => (row.GetIntNotNull(0), row.GetStringNotNull(1)));
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(10);
        await Assert.That(rows.Count).IsEqualTo(10);
        for (var i = 0; i < rows.Count; i++)
        {
            await Assert.That(rows[i].Item1).IsEqualTo(i + 1);
            await Assert.That(rows[i].Item2).IsEqualTo("Regular Query");
        }
    }

    [Test]
    public async Task
        ExecuteQuery_Should_ReturnOneResultSet_When_SimpleQueryStoredProcedureWithOutParameter(CancellationToken ct)
    {
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(OutProcedureCallSimpleQuery);
        using var resultSet = await procedureCall.ExecuteAsync(ct);
        var results = await resultSet.CollectResults(row => (row.GetIntNotNull(0), row.GetStringNotNull(1)));
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(0);
        await Assert.That(rows).IsSingleElement();
        await Assert.That(rows[0].Item1).IsEqualTo(4);
        await Assert.That(rows[0].Item2).IsEqualTo("This is a test");
    }

    [Test]
    public async Task ExecuteQuery_Should_ReturnMultipleResultSet_When_MultiStatement(CancellationToken ct)
    {
        const string multiStatementQuery =
            $"""
             {OutProcedureCallSimpleQuery}
             SELECT 1 test_i, 'test' test_t;
             """;
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery multiStatement = connection.CreateQuery(multiStatementQuery);
        using var resultSet = await multiStatement.ExecuteAsync(ct);
        var results = await resultSet.CollectResults(row => (row.GetIntNotNull(0), row.GetStringNotNull(1)));
        await Assert.That(results.Count).IsEqualTo(2);
        (var firstRows, QueryResult firstResult) = results[0];
        await Assert.That(firstResult.RowsAffected).IsEqualTo(0);
        await Assert.That(firstRows).IsSingleElement();
        await Assert.That(firstRows[0].Item1).IsEqualTo(4);
        await Assert.That(firstRows[0].Item2).IsEqualTo("This is a test");
        (var secondRows, QueryResult secondResult) = results[1];
        await Assert.That(secondResult.RowsAffected).IsEqualTo(1);
        await Assert.That(secondRows).IsSingleElement();
        await Assert.That(secondRows[0].Item1).IsEqualTo(1);
        await Assert.That(secondRows[0].Item2).IsEqualTo("test");
    }

    [Test]
    public async Task ExecuteQuery_Should_Throw_When_SimpleQueryTimesOut(CancellationToken ct)
    {
        const string sleepQuery = "SELECT pg_sleep(5);";
        await using IPgConnection connection = DatabaseFixture.QueryTimeoutPool.CreateConnection();

        using IPgExecutableQuery multiStatement = connection.CreateQuery(sleepQuery);
        var ex = await Assert.ThrowsAsync<PgException>(async () =>
        {
            using var resultSet = await multiStatement.ExecuteAsync(ct);
            await resultSet.CollectResults(row => row.GetIntNotNull(0));
        });
        await Assert.That(ex!.Message).Contains("canceling statement due to statement timeout");
    }
}
