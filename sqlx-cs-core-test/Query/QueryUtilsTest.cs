using JetBrains.Annotations;
using Sqlx.Core.Query;

namespace Sqlx.Core.Query;

[TestSubject(typeof(QueryUtils))]
public class QueryUtilsTest
{
    [Theory]
    [InlineData("SELECT * FROM dbo.test;", 1)]
    [InlineData("SELECT * FROM dbo.test", 1)]
    [InlineData(
        """
        SELECT * FROM dbo.test;
        SELECT * FROM dbo.test2;
        """,
        2)]
    [InlineData(
        """
        
        SELECT * FROM dbo.test;
        SELECT * FROM dbo.test2;
        
        """,
        2)]
    [InlineData(
        """
        SELECT * FROM dbo.test;
        SELECT * FROM dbo.test2
        """,
        2)]
    public void QueryCount_Returns_TotalNumberOfQueriesInBlock(string sql, int expectedCount)
    {
        Assert.Equal(expectedCount, QueryUtils.QueryCount(sql));
    }
}
