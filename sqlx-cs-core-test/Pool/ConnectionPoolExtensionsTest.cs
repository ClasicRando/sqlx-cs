using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Connection;

namespace Sqlx.Core.Pool;

[TestSubject(typeof(ConnectionPoolExtensions))]
public class ConnectionPoolExtensionsTest
{
    [Fact]
    public async Task Begin_Should_GetConnectionAndBeginTransaction()
    {
        var mockPool = Substitute.For<IConnectionPool>();
        var mockConnection = Substitute.For<IConnection>();
        mockPool.CreateConnection().Returns(mockConnection);
        mockConnection.Status.Returns(ConnectionStatus.Closed);

        IConnection _ = await mockPool.Begin();

        mockPool.Received().CreateConnection();
        await mockConnection.Received().OpenAsync(Arg.Any<CancellationToken>());
        await mockConnection.Received().BeginAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public void AcquireAs_Should_GetConnectionAndCast()
    {
        var mockPool = Substitute.For<IConnectionPool>();
        var mockConnection = Substitute.For<IConnection>();
        mockPool.CreateConnection().Returns(mockConnection);

        _ = mockPool.CreateConnectionAs<IConnection>();

        mockPool.Received().CreateConnection();
        mockConnection.Received().Unwrap<IConnection>();
    }
}
