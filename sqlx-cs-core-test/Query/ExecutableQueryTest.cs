using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;
using MockQuery = Sqlx.Core.Query.IExecutableQuery<Sqlx.Core.Result.IDataRow>;

namespace Sqlx.Core.Query;

[TestSubject(typeof(ExecutableQuery))]
public class ExecutableQueryTest
{
    private record TestRow : IFromRow<IDataRow, TestRow>
    {
        public static TestRow FromRow(IDataRow dataRow)
        {
            return new TestRow();
        }
    }

    private record struct TestRowStruct(int Id) : IFromRow<IDataRow, TestRowStruct>
    {
        public static TestRowStruct FromRow(IDataRow dataRow)
        {
            return new TestRowStruct(dataRow.GetIntNotNull(0));
        }
    }

    [Test]
    public async Task ExecuteNonQuery_Should_ReturnTotalAffectedRowCount(CancellationToken ct)
    {
        List<Either<IDataRow, QueryResult>> lst =
        [
            new Either<IDataRow, QueryResult>.Right(new QueryResult(20, string.Empty)),
            new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
        ];
        var query = Substitute.For<MockQuery>();
        query.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

        var rowCount = await query.ExecuteNonQueryAsync(ct);

        await Assert.That(rowCount).IsEqualTo(30);
    }

    [Test]
    [MethodDataSource(nameof(FetchCases))]
    public async Task FetchAll_Should_ReturnAllRowsUntilFirstQueryResult(
        List<Either<IDataRow, QueryResult>> results,
        int expectedRowCount,
        CancellationToken ct)
    {
        var query = Substitute.For<MockQuery>();
        query.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(results.ToAsyncEnumerable()));

        var actualRows =
            await query.FetchAllAsync<IDataRow, TestRow>(ct);

        await Assert.That(actualRows.Count).IsEqualTo(expectedRowCount);
    }

    public static IEnumerable<object[]> FetchCases()
    {
        yield return
        [
            new List<Either<IDataRow, QueryResult>>
            {
                new Either<IDataRow, QueryResult>.Right(new QueryResult(20, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
            },
            0,
        ];
        yield return
        [
            new List<Either<IDataRow, QueryResult>>
            {
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(10, string.Empty)),
            },
            1,
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
            2,
        ];
    }

    public class FetchFirst
    {
        [Test]
        public async Task Throw_When_NoRowsReturned(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var error = await Assert.ThrowsAsync<SqlxException>(async () =>
                await query.FetchFirstAsync<IDataRow, TestRow>(ct));

            await Assert.That(error!.Message).IsEqualTo("Expected at least 1 row but found 0");
        }

        [Test]
        public async Task ReturnFirstRow_When_SingleRowFetched(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            TestRow row = await query.FetchFirstAsync<IDataRow, TestRow>(ct);

            Assert.NotNull(row);
        }

        [Test]
        public async Task ReturnFirstRow_When_MultipleRowsFetched(CancellationToken ct)
        {
            var firstRow = Substitute.For<IDataRow>();
            firstRow.GetIntNotNull(0).Returns(10);
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(firstRow),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(2, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstAsync<IDataRow, TestRowStruct>(ct);

            await Assert.That(row.Id).IsEqualTo(10);
        }
    }

    public class FetchFirstOrDefault
    {
        [Test]
        public async Task ReturnNull_When_ClassAndNoRowsReturned(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRow>(ct);

            Assert.Null(row);
        }

        [Test]
        public async Task ReturnDefault_When_StructAndNoRowsReturned(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRowStruct>(ct);

            await Assert.That(row).IsEqualTo(default(TestRowStruct));
        }

        [Test]
        public async Task ReturnFirstRow_When_SingleRowFetched(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRow>(ct);

            Assert.NotNull(row);
        }

        [Test]
        public async Task ReturnFirstRow_When_MultipleRowsFetched(CancellationToken ct)
        {
            var firstRow = Substitute.For<IDataRow>();
            firstRow.GetIntNotNull(0).Returns(10);
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(firstRow),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(2, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRowStruct>(ct);

            await Assert.That(row.Id).IsEqualTo(10);
        }
    }

    public class FetchSingle
    {
        [Test]
        public async Task Throw_When_NoRowsReturned(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var error = await Assert.ThrowsAsync<SqlxException>(async () =>
                await query.FetchSingleAsync<IDataRow, TestRow>(ct));

            await Assert.That(error!.Message).IsEqualTo("Expected at least 1 row but found 0");
        }

        [Test]
        public async Task ReturnRow_When_SingleRowFetched(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleAsync<IDataRow, TestRow>(ct);

            Assert.NotNull(row);
        }

        [Test]
        public async Task Throw_When_MultipleRowsFetched(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(2, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var error = await Assert.ThrowsAsync<SqlxException>(async () =>
                await query.FetchSingleAsync<IDataRow, TestRow>(ct));

            await Assert.That(error!.Message).IsEqualTo("Expected a single row but found multiple");
        }
    }

    public class FetchSingleOrDefault
    {
        [Test]
        public async Task ReturnNull_When_ClassAndNoRowsReturned(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleOrDefaultAsync<IDataRow, TestRow>(ct);

            Assert.Null(row);
        }

        [Test]
        public async Task ReturnDefault_When_StructAndNoRowsReturned(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleOrDefaultAsync<IDataRow, TestRowStruct>(ct);

            await Assert.That(row).IsEqualTo(default(TestRowStruct));
        }

        [Test]
        public async Task ReturnFirstRow_When_SingleRowFetched(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleOrDefaultAsync<IDataRow, TestRow>(ct);

            Assert.NotNull(row);
        }

        [Test]
        public async Task ReturnFirstRow_When_MultipleRowsFetched(CancellationToken ct)
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(2, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var error = await Assert.ThrowsAsync<SqlxException>(async () =>
                await query.FetchSingleOrDefaultAsync<IDataRow, TestRow>(ct));

            await Assert.That(error!.Message).IsEqualTo("Expected a single row but found multiple");
        }
    }
}