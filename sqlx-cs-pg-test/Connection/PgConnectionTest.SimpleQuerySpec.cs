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
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(simpleQuery);
        var results = await connection.ExecuteQueryAsync(query, ct).CollectResults();
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(10);
        await Assert.That(rows.Count).IsEqualTo(10);
        for (var i = 0; i < rows.Count; i++)
        {
            await Assert.That(rows[i].GetIntNotNull(0)).IsEqualTo(i + 1);
            await Assert.That(rows[i].GetStringNotNull(1)).IsEqualTo("Regular Query");
        }
    }

    [Test]
    public async Task
        ExecuteQuery_Should_ReturnOneResultSet_When_SimpleQueryStoredProcedureWithOutParameter(CancellationToken ct)
    {
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(OutProcedureCallSimpleQuery);
        var results = await connection.ExecuteQueryAsync(procedureCall, ct).CollectResults();
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(0);
        await Assert.That(rows).IsSingleElement();
        await Assert.That(rows[0].GetIntNotNull(0)).IsEqualTo(4);
        await Assert.That(rows[0].GetStringNotNull(1)).IsEqualTo("This is a test");
    }

    [Test]
    public async Task ExecuteQuery_Should_ReturnMultipleResultSet_When_MultiStatement(CancellationToken ct)
    {
        const string multiStatementQuery =
            $"""
             {OutProcedureCallSimpleQuery}
             SELECT 1 test_i;
             """;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery multiStatement = connection.CreateQuery(multiStatementQuery);
        var results = await connection.ExecuteQueryAsync(multiStatement, ct).CollectResults();
        await Assert.That(results.Count).IsEqualTo(2);
        (var firstRows, QueryResult firstResult) = results[0];
        await Assert.That(firstResult.RowsAffected).IsEqualTo(0);
        await Assert.That(firstRows).IsSingleElement();
        await Assert.That(firstRows[0].GetIntNotNull(0)).IsEqualTo(4);
        await Assert.That(firstRows[0].GetStringNotNull(1)).IsEqualTo("This is a test");
        (var secondRows, QueryResult secondResult) = results[1];
        await Assert.That(secondResult.RowsAffected).IsEqualTo(1);
        await Assert.That(secondRows).IsSingleElement();
        await Assert.That(secondRows[0].GetIntNotNull(0)).IsEqualTo(1);
    }

    [Test]
    public async Task ExecuteQuery_Should_Throw_When_SimpleQueryTimesOut(CancellationToken ct)
    {
        const string sleepQuery = "SELECT pg_sleep(5);";
        using IPgConnection connection = DatabaseFixture.QueryTimeoutPool.CreateConnection();

        using IPgExecutableQuery multiStatement = connection.CreateQuery(sleepQuery);
        var ex = await Assert.ThrowsAsync<PgException>(async () =>
        {
            await connection.ExecuteQueryAsync(multiStatement, ct).CollectResults();
        });
        await Assert.That(ex!.Message).Contains("canceling statement due to statement timeout");
    }
}
