using JetBrains.Annotations;
using NSubstitute;
using MockConnection =
    Sqlx.Core.Connection.IConnection<Sqlx.Core.Query.IExecutableQuery<Sqlx.Core.Result.IDataRow>,
        Sqlx.Core.Query.IBindable,
        Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>,
        Sqlx.Core.Result.IDataRow>;
using MockPool =
    Sqlx.Core.Pool.IConnectionPool<
        Sqlx.Core.Connection.IConnection<
            Sqlx.Core.Query.IExecutableQuery<Sqlx.Core.Result.IDataRow>,
            Sqlx.Core.Query.IBindable,
            Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>,
            Sqlx.Core.Result.IDataRow>,
        Sqlx.Core.Query.IBindable,
        Sqlx.Core.Query.IExecutableQuery<Sqlx.Core.Result.IDataRow>,
        Sqlx.Core.Query.IQueryBatch<Sqlx.Core.Query.IBindable, Sqlx.Core.Result.IDataRow>,
        Sqlx.Core.Result.IDataRow>;

namespace Sqlx.Core.Pool;

[TestSubject(typeof(ConnectionPoolExtensions))]
public class ConnectionPoolExtensionsTest
{
    [Fact]
    public async Task Begin_Should_GetConnectionAndBeginTransaction()
    {
        var mockPool = Substitute.For<MockPool>();
        var mockConnection = Substitute.For<MockConnection>();
        mockPool.CreateConnection().Returns(mockConnection);
        mockConnection.Status.Returns(ConnectionStatus.Closed);

        var _ = await mockPool.BeginAsync(TestContext.Current.CancellationToken);

        mockPool.Received().CreateConnection();
        await mockConnection.Received().OpenAsync(Arg.Any<CancellationToken>());
        await mockConnection.Received().BeginAsync(Arg.Any<CancellationToken>());
    }
}
