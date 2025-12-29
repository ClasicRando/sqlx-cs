using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQuery(CancellationToken ct)
    {
        const string extendedQuery =
            """
            SELECT s.s, 'Regular Query' t
            FROM generate_series($1, $2) s
            """;
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(extendedQuery);
        query.Bind(1);
        query.Bind(10);
        var flow = await connection.ExecuteQueryAsync(query, ct);
        var results = await flow.CollectResults();
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
        ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQueryStoredProcedureWithOutParameter(CancellationToken ct)
    {
        const string procedureCallQuery = $"CALL public.{OutProcedureName}($1, $2);";
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(procedureCallQuery);
        procedureCall.Bind((int?)null);
        procedureCall.Bind((int?)null);
        var flow = await connection.ExecuteQueryAsync(procedureCall, ct);
        var results = await flow.CollectResults();
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(0);
        await Assert.That(rows).IsSingleElement();
        await Assert.That(rows[0].GetIntNotNull(0)).IsEqualTo(4);
        await Assert.That(rows[0].GetStringNotNull(1)).IsEqualTo("This is a test");
    }

    [Test]
    public async Task
        ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQueryStoredProcedureWithInOutParameter(CancellationToken ct)
    {
        const string procedureCallQuery = $"CALL public.{InOutProcedureName}($1, $2);";
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(procedureCallQuery);
        procedureCall.Bind(2);
        procedureCall.Bind("start");
        var flow = await connection.ExecuteQueryAsync(procedureCall, ct);
        var results = await flow.CollectResults();
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(0);
        await Assert.That(rows).IsSingleElement();
        await Assert.That(rows[0].GetIntNotNull(0)).IsEqualTo(3);
        await Assert.That(rows[0].GetStringNotNull(1)).IsEqualTo("start,3");
    }

    [Test]
    public async Task ExecuteQuery_Should_Throw_When_ExtendedQueryTimesOut(CancellationToken ct)
    {
        const string sleepQuery = "SELECT pg_sleep($1);";
        using IPgConnection connection = databaseFixture.QueryTimeoutPool.CreateConnection();

        using IPgExecutableQuery sleepStatement = connection.CreateQuery(sleepQuery);
        sleepStatement.Bind(5);
        var ex = await Assert.ThrowsAsync<PgException>(async () =>
        {
            var results = await connection.ExecuteQueryAsync(sleepStatement, ct);
            await results.CollectResults();
        });
        await Assert.That(ex!.Message).Contains("canceling statement due to statement timeout");
    }
}
