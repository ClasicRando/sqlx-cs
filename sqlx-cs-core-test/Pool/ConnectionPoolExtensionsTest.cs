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
        mockPool.Acquire(Arg.Any<CancellationToken>()).Returns(mockConnection);
        mockConnection.Status.Returns(ConnectionStatus.Closed);

        IConnection _ = await mockPool.Begin();

        await mockPool.Received().Acquire(Arg.Any<CancellationToken>());
        await mockConnection.Received().OpenAsync(Arg.Any<CancellationToken>());
        await mockConnection.Received().BeginAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task AcquireAs_Should_GetConnectionAndCast()
    {
        var mockPool = Substitute.For<IConnectionPool>();
        var mockConnection = Substitute.For<IConnection>();
        mockPool.Acquire(Arg.Any<CancellationToken>()).Returns(mockConnection);

        _ = await mockPool.AcquireAs<IConnection>();

        await mockPool.Received().Acquire(Arg.Any<CancellationToken>());
        mockConnection.Received().Unwrap<IConnection>();
    }
}
