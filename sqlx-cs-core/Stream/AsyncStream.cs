using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Stream;

public sealed class AsyncStream : IAsyncStream
{
    private Socket? _socket;
    private System.IO.Stream? _stream;
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

    public bool IsConnected => _socket?.Connected ?? false;

    public async Task OpenAsync(string host, ushort port, CancellationToken cancellationToken)
    {
        var endPoints = await GetIpEndpoints(host, port, cancellationToken);
        for (var i = 0; i < endPoints.Length; i++)
        {
            IPEndPoint ipEndPoint = endPoints[i];
        
            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = false;
            try
            {
                await socket.ConnectAsync(ipEndPoint, cancellationToken);
                _socket = socket;
                _stream = new NetworkStream(_socket);
            }
            catch (Exception e)
            {
                try
                {
                    socket.Dispose();
                }
                catch
                {
                    // ignored
                }
                
                cancellationToken.ThrowIfCancellationRequested();

                if (i == endPoints.Length - 1)
                {
                    throw new SqlxException("Could not connect to host", e);
                }
            }
        }
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        return _stream.WriteAsync(buffer, cancellationToken);
    }

    public async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        var buffer = _arrayPool.Rent(1);
        try
        {
            await _stream.ReadExactlyAsync(buffer.AsMemory(0, 1), cancellationToken);
            return buffer[0];
        }
        finally
        {
            _arrayPool.Return(buffer);
        }
    }

    public async Task<int> ReadIntAsync(CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        var buffer = _arrayPool.Rent(4);
        try
        {
            await _stream.ReadExactlyAsync(buffer.AsMemory(0, 4), cancellationToken);
            return BitConverter.ToInt32(buffer.AsSpan(0, 4));
        }
        finally
        {
            _arrayPool.Return(buffer);
        }
    }

    public ValueTask ReadBuffer(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        return _stream.ReadExactlyAsync(buffer, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        if (_stream is SslStream sslStream)
        {
            try
            {
                await sslStream.ShutdownAsync();
            }
            catch
            {
                // ignored
            }

            try
            {
                sslStream.RemoteCertificate?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        try
        {
            if (_stream is not null)
            {
                await _stream.DisposeAsync();
            }
        }
        catch
        {
            // ignored
        }
        
        if (_socket is not null && _socket.Connected)
        {
            try
            {
                await Task.Run(() => _socket.Dispose(), cancellationToken);
            }
            catch
            {
                // ignored
            }
        }
    }

    private async Task<IPEndPoint[]> GetIpEndpoints(string host, ushort port, CancellationToken cancellationToken)
    {
        var ipAddresses = await Dns.GetHostAddressesAsync(host, cancellationToken)
            .ConfigureAwait(false);
        var ipEndPoints = new IPEndPoint[ipAddresses.Length];
        for (var i = 0; i < ipAddresses.Length; i++)
        {
            ipEndPoints[i] = new IPEndPoint(ipAddresses[i], port);
        }

        return ipEndPoints;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync(CancellationToken.None);
    }
}
