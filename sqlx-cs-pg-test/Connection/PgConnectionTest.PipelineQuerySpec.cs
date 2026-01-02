using Sqlx.Core.Result;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connection;

public partial class PgConnectionTest
{
    [Test]
    public async Task ExecuteQueryBatchAsync_Should_ReturnOneResultSet_When_ExtendedQuery(CancellationToken ct)
    {
        const string sql1 = "SELECT 1 col1;";
        const string sql2 = "SELECT 2 col1;";
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();
        using IPgQueryBatch queryBatch = connection.CreateQueryBatch();
        using IPgBindable query1 = queryBatch.CreateQuery(sql1);
        using IPgBindable query2 = queryBatch.CreateQuery(sql2);
        var results = await connection.ExecuteQueryBatchAsync(queryBatch, ct).CollectResults();
        await Assert.That(results.Count).IsEqualTo(2);
        
        (var rows1, QueryResult result1) = results[0];
        await Assert.That(result1.RowsAffected).IsEqualTo(1);
        await Assert.That(rows1).IsSingleElement();
        await Assert.That(rows1[0].GetIntNotNull(0)).IsEqualTo(1);
        
        (var rows2, QueryResult result2) = results[1];
        await Assert.That(result2.RowsAffected).IsEqualTo(1);
        await Assert.That(rows2).IsSingleElement();
        await Assert.That(rows2[0].GetIntNotNull(0)).IsEqualTo(2);
    }
}
