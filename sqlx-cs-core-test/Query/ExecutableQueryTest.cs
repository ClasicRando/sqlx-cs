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

    [Fact]
    public async Task ExecuteNonQuery_Should_ReturnTotalAffectedRowCount()
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

        var rowCount = await query.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);

        Assert.Equal(30, rowCount);
    }

    [Theory]
    [MemberData(nameof(FetchCases))]
    public async Task FetchAll_Should_ReturnAllRowsUntilFirstQueryResult(
        List<Either<IDataRow, QueryResult>> results,
        int expectedRowCount)
    {
        var query = Substitute.For<MockQuery>();
        query.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(results.ToAsyncEnumerable()));

        var actualRows =
            await query.FetchAllAsync<IDataRow, TestRow>(TestContext.Current.CancellationToken);

        Assert.Equal(expectedRowCount, actualRows.Count);
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
        [Fact]
        public async Task Throw_When_NoRowsReturned()
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
                await query.FetchFirstAsync<IDataRow, TestRow>(TestContext.Current.CancellationToken));

            Assert.Equal("Expected at least 1 row but found 0", error.Message);
        }

        [Fact]
        public async Task ReturnFirstRow_When_SingleRowFetched()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstAsync<IDataRow, TestRow>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(row);
        }

        [Fact]
        public async Task ReturnFirstRow_When_MultipleRowsFetched()
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

            var row = await query.FetchFirstAsync<IDataRow, TestRowStruct>(
                TestContext.Current.CancellationToken);

            Assert.Equal(10, row.Id);
        }
    }

    public class FetchFirstOrDefault
    {
        [Fact]
        public async Task ReturnNull_When_ClassAndNoRowsReturned()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRow>(
                TestContext.Current.CancellationToken);

            Assert.Null(row);
        }

        [Fact]
        public async Task ReturnDefault_When_StructAndNoRowsReturned()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRowStruct>(
                TestContext.Current.CancellationToken);

            Assert.Equal(default, row);
        }

        [Fact]
        public async Task ReturnFirstRow_When_SingleRowFetched()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRow>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(row);
        }

        [Fact]
        public async Task ReturnFirstRow_When_MultipleRowsFetched()
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

            var row = await query.FetchFirstOrDefaultAsync<IDataRow, TestRowStruct>(
                TestContext.Current.CancellationToken);

            Assert.Equal(10, row.Id);
        }
    }

    public class FetchSingle
    {
        [Fact]
        public async Task Throw_When_NoRowsReturned()
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
                await query.FetchSingleAsync<IDataRow, TestRow>(TestContext.Current.CancellationToken));

            Assert.Equal("Expected at least 1 row but found 0", error.Message);
        }

        [Fact]
        public async Task ReturnRow_When_SingleRowFetched()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleAsync<IDataRow, TestRow>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(row);
        }

        [Fact]
        public async Task Throw_When_MultipleRowsFetched()
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
                await query.FetchSingleAsync<IDataRow, TestRow>(TestContext.Current.CancellationToken));

            Assert.Equal("Expected a single row but found multiple", error.Message);
        }
    }

    public class FetchSingleOrDefault
    {
        [Fact]
        public async Task ReturnNull_When_ClassAndNoRowsReturned()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleOrDefaultAsync<IDataRow, TestRow>(
                TestContext.Current.CancellationToken);

            Assert.Null(row);
        }

        [Fact]
        public async Task ReturnDefault_When_StructAndNoRowsReturned()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Right(new QueryResult(0, string.Empty)),
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleOrDefaultAsync<IDataRow, TestRowStruct>(
                TestContext.Current.CancellationToken);

            Assert.Equal(default, row);
        }

        [Fact]
        public async Task ReturnFirstRow_When_SingleRowFetched()
        {
            List<Either<IDataRow, QueryResult>> lst =
            [
                new Either<IDataRow, QueryResult>.Left(Substitute.For<IDataRow>()),
                new Either<IDataRow, QueryResult>.Right(new QueryResult(1, string.Empty)),
            ];
            var query = Substitute.For<MockQuery>();
            query.ExecuteAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(lst.ToAsyncEnumerable()));

            var row = await query.FetchSingleOrDefaultAsync<IDataRow, TestRow>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(row);
        }

        [Fact]
        public async Task ReturnFirstRow_When_MultipleRowsFetched()
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
                await query.FetchSingleOrDefaultAsync<IDataRow, TestRow>(
                    TestContext.Current.CancellationToken));

            Assert.Equal("Expected a single row but found multiple", error.Message);
        }
    }
}
