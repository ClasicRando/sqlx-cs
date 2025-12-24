using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    private const string OutProcedureCallSimpleQuery =
        $"CALL public.{OutProcedureName}(null, null);";

    [Fact]
    public async Task ExecuteQuery_Should_ReturnOneResultSet_When_SimpleQuery()
    {
        const string simpleQuery =
            """
            SELECT s.s, 'Regular Query' t
            FROM generate_series(1, 10) s
            """;
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(simpleQuery);
        var flow = await connection.ExecuteQueryAsync(query, TestContext.Current.CancellationToken);
        var results = await flow.CollectResults();
        Assert.Single(results);
        (var rows, QueryResult result) = results[0];
        Assert.Equal(10, result.RowsAffected);
        Assert.Equal(10, rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            Assert.Equal(i + 1, rows[i].GetIntNotNull(0));
            Assert.Equal("Regular Query", rows[i].GetStringNotNull(1));
        }
    }

    [Fact]
    public async Task
        ExecuteQuery_Should_ReturnOneResultSet_When_SimpleQueryStoredProcedureWithOutParameter()
    {
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(OutProcedureCallSimpleQuery);
        var flow = await connection.ExecuteQueryAsync(
            procedureCall,
            TestContext.Current.CancellationToken);
        var results = await flow.CollectResults();
        Assert.Single(results);
        (var rows, QueryResult result) = results[0];
        Assert.Equal(0, result.RowsAffected);
        Assert.Single(rows);
        Assert.Equal(4, rows[0].GetIntNotNull(0));
        Assert.Equal("This is a test", rows[0].GetStringNotNull(1));
    }

    [Fact]
    public async Task ExecuteQuery_Should_ReturnMultipleResultSet_When_MultiStatement()
    {
        const string multiStatementQuery =
            $"""
             {OutProcedureCallSimpleQuery}
             SELECT 1 test_i;
             """;
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery multiStatement = connection.CreateQuery(multiStatementQuery);
        var flow = await connection.ExecuteQueryAsync(
            multiStatement,
            TestContext.Current.CancellationToken);
        var results = await flow.CollectResults();
        Assert.Equal(2, results.Count);
        (var firstRows, QueryResult firstResult) = results[0];
        Assert.Equal(0, firstResult.RowsAffected);
        Assert.Single(firstRows);
        Assert.Equal(4, firstRows[0].GetIntNotNull(0));
        Assert.Equal("This is a test", firstRows[0].GetStringNotNull(1));
        (var secondRows, QueryResult secondResult) = results[1];
        Assert.Equal(1, secondResult.RowsAffected);
        Assert.Single(secondRows);
        Assert.Equal(1, secondRows[0].GetIntNotNull(0));
    }

    [Fact]
    public async Task ExecuteQuery_Should_Throw_When_SimpleQueryTimesOut()
    {
        const string sleepQuery = "SELECT pg_sleep(5);";
        using IPgConnection connection = _databaseFixture.QueryTimeoutPool.CreateConnection();

        using IPgExecutableQuery multiStatement = connection.CreateQuery(sleepQuery);
        var ex = await Assert.ThrowsAsync<PgException>(async () =>
        {
            var results = await connection.ExecuteQueryAsync(
                multiStatement,
                TestContext.Current.CancellationToken);
            await results.CollectResults();
        });
        Assert.Contains("canceling statement due to statement timeout", ex.Message);
    }
}
