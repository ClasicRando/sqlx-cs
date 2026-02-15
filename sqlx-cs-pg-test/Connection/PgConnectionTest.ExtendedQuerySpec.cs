using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQuery(
        CancellationToken ct)
    {
        const string extendedQuery =
            """
            SELECT s.s, 'Regular Query' t
            FROM generate_series($1, $2) s
            """;
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(extendedQuery);
        query.Bind(1);
        query.Bind(10);
        using var resultSet = await connection.ExecuteQueryAsync(query, ct);
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
        ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQueryStoredProcedureWithOutParameter(
            CancellationToken ct)
    {
        const string procedureCallQuery = $"CALL public.{OutProcedureName}($1, $2);";
        using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(procedureCallQuery);
        procedureCall.Bind((int?)null);
        procedureCall.Bind((int?)null);
        using var resultSet = await connection.ExecuteQueryAsync(procedureCall, ct);
        var results = await resultSet.CollectResults(row => (row.GetIntNotNull(0), row.GetStringNotNull(1)));
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(0);
        await Assert.That(rows).IsSingleElement();
        await Assert.That(rows[0].Item1).IsEqualTo(4);
        await Assert.That(rows[0].Item2).IsEqualTo("This is a test");
    }

    [Test]
    public async Task
        ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQueryStoredProcedureWithInOutParameter(
            CancellationToken ct)
    {
        const string procedureCallQuery = $"CALL public.{InOutProcedureName}($1, $2);";
        await using IPgConnection connection = DatabaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(procedureCallQuery);
        procedureCall.Bind(2);
        procedureCall.Bind("start");
        using var resultSet = await connection.ExecuteQueryAsync(procedureCall, ct);
        var results = await resultSet.CollectResults(row => (row.GetIntNotNull(0), row.GetStringNotNull(1)));
        await Assert.That(results).IsSingleElement();
        (var rows, QueryResult result) = results[0];
        await Assert.That(result.RowsAffected).IsEqualTo(0);
        await Assert.That(rows).IsSingleElement();
        await Assert.That(rows[0].Item1).IsEqualTo(3);
        await Assert.That(rows[0].Item2).IsEqualTo("start,3");
    }

    [Test]
    public async Task ExecuteQuery_Should_Throw_When_ExtendedQueryTimesOut(CancellationToken ct)
    {
        const string sleepQuery = "SELECT pg_sleep($1);";
        using IPgConnection connection = DatabaseFixture.QueryTimeoutPool.CreateConnection();

        using IPgExecutableQuery sleepStatement = connection.CreateQuery(sleepQuery);
        sleepStatement.Bind(5);
        var ex = await Assert.ThrowsAsync<PgException>(async () =>
        {
            var resultSet = await connection.ExecuteQueryAsync(sleepStatement, ct);
            await resultSet.CollectResults(row => row.GetIntNotNull(0));
        });
        await Assert.That(ex!.Message).Contains("canceling statement due to statement timeout");
    }
}
