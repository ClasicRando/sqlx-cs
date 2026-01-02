using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Result;
using MockQueryBatch = Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>;

namespace Sqlx.Core.Query;

[TestSubject(typeof(QueryBatch))]
public class QueryBatchTest
{
    [Test]
    [MethodDataSource(nameof(ExecuteNonQueryAsyncCases))]
    public async Task ExecuteNonQueryAsync_Should_ReturnTotalAffectedRowCount(
        List<Either<IDataRow, QueryResult>> lst,
        int numberOfAffectedRows,
        CancellationToken ct)
    {
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncEnumerable());

        var rowCount = await query.ExecuteNonQueryAsync(ct);
        
        await Assert.That(rowCount).IsEqualTo(numberOfAffectedRows);
    }
    
    public static IEnumerable<Func<(List<Either<IDataRow, QueryResult>>, int)>> ExecuteNonQueryAsyncCases()
    {
        yield return () =>
        (
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(20, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
            ],
            30
        );
        yield return () =>
        (
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
            ],
            10
        );
        yield return () =>
        (
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(5, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(5, string.Empty)),
            ],
            10
        );
    }
    
    [Test]
    public async Task ToResultAsync_Should_ExtractRows(CancellationToken ct)
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
            .Returns(lst.ToAsyncEnumerable());

        var batchResult = query.ToResult(ct);

        var rows1 = await batchResult.ExtractNextResultAsync<Row1>();
        await Assert.That(rows1).IsSingleElement();
        
        var rows2 = await batchResult.ExtractNextResultAsync<Row2>();
        await Assert.That(rows2).IsSingleElement();

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
