using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace Sqlx.Core.Stream;

[TestSubject(typeof(AsyncStream))]
public class AsyncStreamTest
{
    [Fact]
    public async Task WriteAsync_Should_SendAllBytes()
    {
        const int port = 8080;
        byte[] expectedBytes = [0xA5, 0x45, 0xFF];
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        var listenerTask = Task.Run(ListenTask, TestContext.Current.CancellationToken);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, TestContext.Current.CancellationToken);
        await asyncStream.WriteAsync(expectedBytes, TestContext.Current.CancellationToken);

        var actualBytes = await listenerTask;
        
        Assert.Equal(expectedBytes, actualBytes);
        return;

        async Task<byte[]> ListenTask()
        {
            TcpClient client = await listener.AcceptTcpClientAsync(TestContext.Current.CancellationToken);
            await using NetworkStream stream = client.GetStream();

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);
            return buffer[..bytesRead];
        }
    }
    
    [Theory]
    [MemberData(nameof(ReadBufferAsyncCases))]
    public async Task ReadBufferAsync_Should_ReadAllBytes_When_SimplePayload(byte[] expectedBytes)
    {
        const int port = 8080;
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Task _ = Task.Run(ListenTask, TestContext.Current.CancellationToken);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, TestContext.Current.CancellationToken);

        var actualBytes = new byte[expectedBytes.Length];
        await asyncStream.ReadBufferAsync(actualBytes, TestContext.Current.CancellationToken);
        
        Assert.Equal(expectedBytes, actualBytes);
        return;

        async Task ListenTask()
        {
            listener.Start();
            TcpClient client = await listener.AcceptTcpClientAsync(TestContext.Current.CancellationToken);
            await using NetworkStream stream = client.GetStream();

            await stream.WriteAsync(expectedBytes, TestContext.Current.CancellationToken);
        }
    }

    public static IEnumerable<TheoryDataRow<byte[]>> ReadBufferAsyncCases()
    {
        return new TheoryData<byte[]>(
            ([0xA5, 0x45, 0xFF]),
            (Enumerable.Range(1, AsyncStream.DefaultBufferSize + 1).Select(i => (byte)i).ToArray()));
    }
    
    [Fact]
    public async Task ReadBufferAsync_Should_ReadAllBytesGrowAndShrinkInternalBuffer_When_OneTimeLargePayload()
    {
        const int port = 8080;
        var largePacket = Enumerable.Range(1, AsyncStream.DefaultBufferSize + 1)
            .Select(i => (byte)i).ToArray();
        byte[] smallPacket = [0xA5, 0x45, 0xFF];
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Task _ = Task.Run(ListenTask, TestContext.Current.CancellationToken);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, TestContext.Current.CancellationToken);

        var actualBytes = new byte[largePacket.Length];
        await asyncStream.ReadBufferAsync(actualBytes, TestContext.Current.CancellationToken);
        Assert.Equal(largePacket, actualBytes);
        Assert.True(asyncStream.InnerBufferSize > AsyncStream.DefaultBufferSize);
        
        actualBytes = new byte[smallPacket.Length];
        await asyncStream.ReadBufferAsync(actualBytes, TestContext.Current.CancellationToken);
        Assert.Equal(smallPacket, actualBytes);
        Assert.False(asyncStream.InnerBufferSize > AsyncStream.DefaultBufferSize);
        
        return;

        async Task ListenTask()
        {
            listener.Start();
            TcpClient client = await listener.AcceptTcpClientAsync(TestContext.Current.CancellationToken);
            await using NetworkStream stream = client.GetStream();

            await stream.WriteAsync(largePacket, TestContext.Current.CancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
            await stream.WriteAsync(smallPacket, TestContext.Current.CancellationToken);
        }
    }
    
    [Fact]
    public async Task ReadBufferAsync_Should_ReadAllBytes_When_ComplexPayload()
    {
        const int port = 8080;
        byte[] firstPacket = [0xB8, 0xC2, 0x89];
        byte[] secondPacket = [0xA5, 0x45, 0xFF];
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Task _ = Task.Run(ListenTask, TestContext.Current.CancellationToken);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, TestContext.Current.CancellationToken);

        var firstByte = await asyncStream.ReadByteAsync(TestContext.Current.CancellationToken);
        Assert.Equal(firstPacket[0], firstByte);
        var firstInt = await asyncStream.ReadIntAsync(TestContext.Current.CancellationToken);
        Assert.Equal(-1031166651, firstInt);
        var secondByte = await asyncStream.ReadByteAsync(TestContext.Current.CancellationToken);
        Assert.Equal(secondPacket[2], secondByte);
        return;

        async Task ListenTask()
        {
            listener.Start();
            TcpClient client = await listener.AcceptTcpClientAsync(TestContext.Current.CancellationToken);
            await using NetworkStream stream = client.GetStream();

            await stream.WriteAsync(firstPacket, TestContext.Current.CancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
            await stream.WriteAsync(secondPacket, TestContext.Current.CancellationToken);
        }
    }
}
