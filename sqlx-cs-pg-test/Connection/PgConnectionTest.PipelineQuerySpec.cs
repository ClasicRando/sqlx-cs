using Sqlx.Core.Result;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Fact]
    public async Task ExecuteQueryBatchAsync_Should_ReturnOneResultSet_When_ExtendedQuery()
    {
        const string sql1 = "SELECT 1 col1;";
        const string sql2 = "SELECT 2 col1;";
        using IPgConnection connection = _databaseFixture.BasicPool.CreateConnection();
        using IPgQueryBatch queryBatch = connection.CreateQueryBatch();
        using IPgBindable query1 = queryBatch.CreateQuery(sql1);
        using IPgBindable query2 = queryBatch.CreateQuery(sql2);
        var flow = await connection.ExecuteQueryBatchAsync(queryBatch, TestContext.Current.CancellationToken);
        var results = await flow.CollectResults();
        Assert.Equal(2, results.Count);
        
        (var rows1, QueryResult result1) = results[0];
        Assert.Equal(1, result1.RowsAffected);
        Assert.Single(rows1);
        Assert.Equal(1, rows1[0].GetIntNotNull(0));
        
        (var rows2, QueryResult result2) = results[1];
        Assert.Equal(1, result2.RowsAffected);
        Assert.Single(rows2);
        Assert.Equal(2, rows2[0].GetIntNotNull(0));
    }
}
