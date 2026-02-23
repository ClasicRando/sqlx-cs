using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Query;
using MockQueryBatch = Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>;

namespace Sqlx.Core.Result;

[TestSubject(typeof(AsyncResultSet))]
public class AsyncResultSetTest
{
    [Test]
    public async Task ExtractAllRowsAffected_Should_ExtractRows(CancellationToken ct)
    {
        List<long> expected = [8, 5];
        List<Either<IDataRow, QueryResult>> lst =
        [
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(8, string.Empty)),
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
        ];
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncResultSet());

        var batchResult = await query.ExecuteBatch(ct);

        var rowsAffected = await batchResult.ExtractAllRowsAffected(ct);
        await Assert.That(rowsAffected).IsEquivalentTo(expected);
    }
    
    [Test]
    public async Task CombineAllRowsAffected_Should_ExtractRows(CancellationToken ct)
    {
        const long expected = 13;
        List<Either<IDataRow, QueryResult>> lst =
        [
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(8, string.Empty)),
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
        ];
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncResultSet());

        var batchResult = await query.ExecuteBatch(ct);

        var rowsAffected = await batchResult.CombineAllRowsAffected(ct);
        await Assert.That(rowsAffected).IsEqualTo(expected);
    }

    [Test]
    public async Task FetchNextResultAsync_Should_ExtractRows(CancellationToken ct)
    {
        List<Either<IDataRow, QueryResult>> lst =
        [
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
        ];
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncResultSet());

        var batchResult = await query.ExecuteBatch(ct);

        var rows1 = await batchResult.FetchNextResultAsync<IDataRow, Row1>(ct).ToListAsync(ct);
        await Assert.That(rows1).IsSingleElement();
        
        var rows2 = await batchResult.FetchNextResultAsync<IDataRow,Row2>(ct).ToListAsync(ct);
        await Assert.That(rows2).IsSingleElement();

        await Assert.ThrowsAsync<InvalidOperationException>(() => batchResult.ExtractNextResultAsync<IDataRow,Row1>(ct));
    }

    [Test]
    public async Task ExtractNextResultAsync_Should_ExtractRows(CancellationToken ct)
    {
        List<Either<IDataRow, QueryResult>> lst =
        [
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
            Either.Left<IDataRow, QueryResult>(Substitute.For<IDataRow>()),
            Either.Right<IDataRow, QueryResult>(new QueryResult(5, string.Empty)),
        ];
        var query = Substitute.For<MockQueryBatch>();
        query.ExecuteBatch(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncResultSet());

        var batchResult = await query.ExecuteBatch(ct);

        var rows1 = await batchResult.ExtractNextResultAsync<IDataRow, Row1>(ct);
        await Assert.That(rows1).IsSingleElement();
        
        var rows2 = await batchResult.ExtractNextResultAsync<IDataRow,Row2>(ct);
        await Assert.That(rows2).IsSingleElement();

        await Assert.ThrowsAsync<InvalidOperationException>(() => batchResult.ExtractNextResultAsync<IDataRow,Row1>(ct));
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
