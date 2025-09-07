using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

[TestSubject(typeof(QueryBatch))]
public class QueryBatchTest
{
    [Theory]
    [MemberData(nameof(ExecuteNonQueryCases))]
    public async Task ExecuteNonQuery_Should_ReturnTotalAffectedRowCount(
        List<Either<IDataRow, QueryResult>> lst,
        int numberOfAffectedRows)
    {
        var query = Substitute.For<IQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

        var rowCount = await query.ExecuteNonQuery();
        
        Assert.Equal(numberOfAffectedRows, rowCount);
    }
    
    public static IEnumerable<object[]> ExecuteNonQueryCases()
    {
        yield return
        [
            new List<Either<IDataRow, QueryResult>>
            {
                new Either<IDataRow, QueryResult>.Right(new QueryResult(20, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
            },
            30,
        ];
        yield return
        [
            new List<Either<IDataRow, QueryResult>>
            {
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
            },
            10,
        ];
        yield return
        [
            new List<Either<IDataRow, QueryResult>>
            {
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            },
            10,
        ];
    }
}
