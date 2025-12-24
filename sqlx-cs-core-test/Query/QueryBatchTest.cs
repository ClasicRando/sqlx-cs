using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Result;
using MockQueryBatch = Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>;

namespace Sqlx.Core.Query;

[TestSubject(typeof(QueryBatch))]
public class QueryBatchTest
{
    [Theory]
    [MemberData(nameof(ExecuteNonQueryAsyncCases))]
    public async Task ExecuteNonQueryAsync_Should_ReturnTotalAffectedRowCount(
        List<Either<IDataRow, QueryResult>> lst,
        int numberOfAffectedRows)
    {
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

        var rowCount = await query.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        
        Assert.Equal(numberOfAffectedRows, rowCount);
    }
    
    public static IEnumerable<TheoryDataRow<List<Either<IDataRow, QueryResult>>, int>> ExecuteNonQueryAsyncCases()
    {
        return new TheoryData<List<Either<IDataRow, QueryResult>>, int>(
            (
                [
                    new Either<IDataRow, QueryResult>.Right(new QueryResult(20, string.Empty)),
                    new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                    new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
                ],
                30
            ),
            (
                [
                    new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                    new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
                ],
                10
            ),
            (
                [
                    new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                    new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                    new Either<IDataRow, QueryResult>.Right(new QueryResult(5, string.Empty)),
                    new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                    new Either<IDataRow, QueryResult>.Right(new QueryResult(5, string.Empty)),
                ],
                10
            ));
    }
    
    [Fact]
    public async Task ToResultAsync_Should_ExtractRows()
    {
        List<Either<IDataRow, QueryResult>> lst =
        [
            new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            new Either<IDataRow, QueryResult>.Right(new QueryResult(5, string.Empty)),
            new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            new Either<IDataRow, QueryResult>.Right(new QueryResult(5, string.Empty)),
        ];
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

        var batchResult = await query.ToResultAsync(TestContext.Current.CancellationToken);

        var rows1 = await batchResult.ExtractNextResultAsync<Row1>();
        Assert.Single(rows1);
        
        var rows2 = await batchResult.ExtractNextResultAsync<Row2>();
        Assert.Single(rows2);

        await Assert.ThrowsAsync<QueryBatchExhausted>(() => batchResult.ExtractNextResultAsync<Row1>());
    }

    private struct Row1 : IFromRow<IDataRow, Row1>
    {
        public static Row1 FromRow(IDataRow dataRow)
        {
            return new Row1();
        }
    }

    private struct Row2 : IFromRow<IDataRow, Row2>
    {
        public static Row2 FromRow(IDataRow dataRow)
        {
            return new Row2();
        }
    }
}
