using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQuery()
    {
        const string extendedQuery =
            """
            SELECT s.s, 'Regular Query' t
            FROM generate_series($1, $2) s
            """;
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgExecutableQuery query = connection.CreateQuery(extendedQuery);
        query.Bind(1);
        query.Bind(10);
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
        ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQueryStoredProcedureWithOutParameter()
    {
        const string procedureCallQuery = $"CALL public.{OutProcedureName}($1, $2);";
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(procedureCallQuery);
        procedureCall.Bind((int?)null);
        procedureCall.Bind((int?)null);
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
    public async Task
        ExecuteQuery_Should_ReturnOneResultSet_When_ExtendedQueryStoredProcedureWithInOutParameter()
    {
        const string procedureCallQuery = $"CALL public.{InOutProcedureName}($1, $2);";
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();

        using IPgExecutableQuery procedureCall = connection.CreateQuery(procedureCallQuery);
        procedureCall.Bind(2);
        procedureCall.Bind("start");
        var flow = await connection.ExecuteQueryAsync(
            procedureCall,
            TestContext.Current.CancellationToken);
        var results = await flow.CollectResults();
        Assert.Single(results);
        (var rows, QueryResult result) = results[0];
        Assert.Equal(0, result.RowsAffected);
        Assert.Single(rows);
        Assert.Equal(3, rows[0].GetIntNotNull(0));
        Assert.Equal("start,3", rows[0].GetStringNotNull(1));
    }

    [Fact]
    public async Task ExecuteQuery_Should_Throw_When_ExtendedQueryTimesOut()
    {
        const string sleepQuery = "SELECT pg_sleep($1);";
        using IPgConnection connection = _databaseFixture.QueryTimeoutPool.CreateConnection();

        using IPgExecutableQuery sleepStatement = connection.CreateQuery(sleepQuery);
        sleepStatement.Bind(5);
        var ex = await Assert.ThrowsAsync<PgException>(async () =>
        {
            var results = await connection.ExecuteQueryAsync(
                sleepStatement,
                TestContext.Current.CancellationToken);
            await results.CollectResults();
        });
        Assert.Contains("canceling statement due to statement timeout", ex.Message);
    }
}
