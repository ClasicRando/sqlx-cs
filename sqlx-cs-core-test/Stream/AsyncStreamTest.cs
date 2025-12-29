using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace Sqlx.Core.Stream;

[TestSubject(typeof(AsyncStream))]
public class AsyncStreamTest
{
    private const string TestKey = "AsyncStreamTest";
    
    [Test]
    [NotInParallel(TestKey)]
    public async Task WriteAsync_Should_SendAllBytes(CancellationToken ct)
    {
        const int port = 8080;
        byte[] expectedBytes = [0xA5, 0x45, 0xFF];
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        var listenerTask = Task.Run(ListenTask, ct);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, ct);
        await asyncStream.WriteAsync(expectedBytes, ct);

        var actualBytes = await listenerTask;
        
        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
        return;

        async Task<byte[]> ListenTask()
        {
            TcpClient client = await listener.AcceptTcpClientAsync(ct);
            await using NetworkStream stream = client.GetStream();

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer, ct);
            return buffer[..bytesRead];
        }
    }
    
    [Test]
    [NotInParallel(TestKey)]
    [MethodDataSource(nameof(ReadBufferAsyncCases))]
    public async Task ReadBufferAsync_Should_ReadAllBytes_When_SimplePayload(
        byte[] expectedBytes,
        CancellationToken ct)
    {
        const int port = 8080;
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Task _ = Task.Run(ListenTask, ct);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, ct);

        var actualBytes = new byte[expectedBytes.Length];
        await asyncStream.ReadBufferAsync(actualBytes, ct);
        
        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
        return;

        async Task ListenTask()
        {
            listener.Start();
            TcpClient client = await listener.AcceptTcpClientAsync(ct);
            await using NetworkStream stream = client.GetStream();

            await stream.WriteAsync(expectedBytes, ct);
        }
    }

    public static IEnumerable<Func<byte[]>> ReadBufferAsyncCases()
    {
        yield return () => [0xA5, 0x45, 0xFF];
        yield return () => Enumerable.Range(1, AsyncStream.DefaultBufferSize + 1).Select(i => (byte)i).ToArray();
    }
    
    [Test]
    [NotInParallel(TestKey)]
    public async Task ReadBufferAsync_Should_ReadAllBytesGrowAndShrinkInternalBuffer_When_OneTimeLargePayload(
        CancellationToken ct)
    {
        const int port = 8080;
        var largePacket = Enumerable.Range(1, AsyncStream.DefaultBufferSize + 1)
            .Select(i => (byte)i).ToArray();
        byte[] smallPacket = [0xA5, 0x45, 0xFF];
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Task _ = Task.Run(ListenTask, ct);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, ct);

        var actualBytes = new byte[largePacket.Length];
        await asyncStream.ReadBufferAsync(actualBytes, ct);
        await Assert.That(actualBytes).IsEquivalentTo(largePacket);
        await Assert.That(asyncStream.InnerBufferSize > AsyncStream.DefaultBufferSize).IsTrue();
        
        actualBytes = new byte[smallPacket.Length];
        await asyncStream.ReadBufferAsync(actualBytes, ct);
        await Assert.That(actualBytes).IsEquivalentTo(smallPacket);
        await Assert.That(asyncStream.InnerBufferSize > AsyncStream.DefaultBufferSize).IsFalse();
        
        return;

        async Task ListenTask()
        {
            listener.Start();
            TcpClient client = await listener.AcceptTcpClientAsync(ct);
            await using NetworkStream stream = client.GetStream();

            await stream.WriteAsync(largePacket, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
            await stream.WriteAsync(smallPacket, ct);
        }
    }
    
    [Test]
    [NotInParallel(TestKey)]
    public async Task ReadBufferAsync_Should_ReadAllBytes_When_ComplexPayload(
        CancellationToken ct)
    {
        const int port = 8080;
        byte[] firstPacket = [0xB8, 0xC2, 0x89];
        byte[] secondPacket = [0xA5, 0x45, 0xFF];
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Task _ = Task.Run(ListenTask, ct);

        using var asyncStream = new AsyncStream();
        await asyncStream.OpenAsync("127.0.0.1", port, ct);

        var firstByte = await asyncStream.ReadByteAsync(ct);
        await Assert.That(firstByte).IsEqualTo(firstPacket[0]);
        var firstInt = await asyncStream.ReadIntAsync(ct);
        await Assert.That(firstInt).IsEqualTo(-1031166651);
        var secondByte = await asyncStream.ReadByteAsync(ct);
        await Assert.That(secondByte).IsEqualTo(secondPacket[2]);
        return;

        async Task ListenTask()
        {
            listener.Start();
            TcpClient client = await listener.AcceptTcpClientAsync(ct);
            await using NetworkStream stream = client.GetStream();

            await stream.WriteAsync(firstPacket, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
            await stream.WriteAsync(secondPacket, ct);
        }
    }
}
