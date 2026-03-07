using JetBrains.Annotations;
using Sqlx.Core.Query;

namespace Sqlx.Core.Query;

[TestSubject(typeof(QueryUtils))]
public class QueryUtilsTest
{
    [Test]
    [Arguments("SELECT * FROM dbo.test;", 1)]
    [Arguments("SELECT * FROM dbo.test", 1)]
    [Arguments(
        """
        SELECT * FROM dbo.test;
        SELECT * FROM dbo.test2;
        """,
        2)]
    [Arguments(
        """
        
        SELECT * FROM dbo.test;
        SELECT * FROM dbo.test2;
        
        """,
        2)]
    [Arguments(
        """
        SELECT * FROM dbo.test;
        SELECT * FROM dbo.test2
        """,
        2)]
    public async Task QueryCount_Returns_TotalNumberOfQueriesInBlock(string sql, int expectedCount)
    {
        await Assert.That(QueryUtils.QueryCount(sql)).IsEqualTo(expectedCount);
    }
}