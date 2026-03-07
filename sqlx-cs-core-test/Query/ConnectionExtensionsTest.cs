using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Connection;
using Sqlx.Core.Result;
using MockQuery = Sqlx.Core.Query.IExecutableQuery<Sqlx.Core.Result.IDataRow>;
using MockQueryBatch = Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>;
using MockConnection = Sqlx.Core.Connection.IConnection<Sqlx.Core.Query.IExecutableQuery<Sqlx.Core.Result.IDataRow>, Sqlx.Core.Query.IBindable, Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>, Sqlx.Core.Result.IDataRow>;

namespace Sqlx.Core.Query;

[TestSubject(typeof(Connection.ConnectionExtensions))]
public class ConnectionExtensionsTest
{
    [Test]
    public async Task ExecuteNonQueryBatchAsync_Should_ReturnRowsAffectedCount()
    {
        List<Either<IDataRow, QueryResult>> lst =
        [
            Either.Right<IDataRow, QueryResult>(new QueryResult(20, string.Empty)),
            Either.Right<IDataRow, QueryResult>(new QueryResult(10, string.Empty)),
        ];
        var bindMany = Substitute.For<IBindMany<IBindable>>();
        var query = Substitute.For<MockQuery>();
        var queryBatch = Substitute.For<MockQueryBatch>();
        var connection = Substitute.For<MockConnection>();
        queryBatch.ExecuteBatchAsync(Arg.Any<CancellationToken>())
            .Returns(lst.ToAsyncResultSet());
        connection.CreateQueryBatch().Returns(queryBatch);
        queryBatch.CreateQuery(Arg.Any<string>()).Returns(query);

        var rowCount = await connection.ExecuteNonQueryBatchAsync(string.Empty, [bindMany]);

        await Assert.That(rowCount).IsEqualTo(30);
        connection.CreateQueryBatch().Received(1);
        queryBatch.CreateQuery(Arg.Any<string>()).Received(1);
        bindMany.Received().BindMany(Arg.Is<IBindable>(query));
    }
}
