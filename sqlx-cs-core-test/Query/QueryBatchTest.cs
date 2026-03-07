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
        query.ExecuteBatchAsync(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncResultSet());

        var rowCount = await query.ExecuteNonQueryAsync(ct);
        
        await Assert.That(rowCount).IsEqualTo(numberOfAffectedRows);
    }
    
    public static IEnumerable<Func<(List<Either<IDataRow, QueryResult>>, int)>> ExecuteNonQueryAsyncCases()
    {
        yield return () =>
        (
            [
                Either.Right<IDataRow, QueryResult>(new QueryResult(20, string.Empty)),
                Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
                Either.Right<IDataRow, QueryResult>(new QueryResult(10, string.Empty)),
            ],
            30
        );
        yield return () =>
        (
            [
                Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
                Either.Right<IDataRow, QueryResult>(new QueryResult(10, string.Empty)),
            ],
            10
        );
        yield return () =>
        (
            [
                Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
                Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
                Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
                Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
                Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
            ],
            10
        );
    }
}
