using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Stream;

public sealed class AsyncStream : IAsyncStream
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    private const int DefaultBufferSize = 1024 * 8;
    private Socket? _socket;
    private System.IO.Stream? _stream;

    private byte[] _innerBuffer = ArrayPool.Rent(DefaultBufferSize);
    private int _bufferPosition;
    private int _bufferLength;

    public bool IsConnected => _socket?.Connected ?? false;

    public async Task OpenAsync(string host, ushort port, CancellationToken cancellationToken)
    {
        await CloseAsync(cancellationToken);
        var endPoints = await GetIpEndpoints(host, port, cancellationToken);
        for (var i = 0; i < endPoints.Length; i++)
        {
            IPEndPoint ipEndPoint = endPoints[i];
        
            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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

    private async ValueTask FillBuffer(int length)
    {
        SqlxException.ThrowIfNull(_stream);
        var bytesRemaining = _bufferLength - _bufferPosition;
        if (bytesRemaining >= length)
        {
            return;
        }

        if (_bufferLength > 0)
        {
            _innerBuffer.AsSpan()[_bufferPosition.._bufferLength]
                .CopyTo(_innerBuffer);
            _bufferLength = bytesRemaining;
        }

        _bufferPosition = 0;
        var count = length - bytesRemaining;
        while (count > 0)
        {
            var bytesRead = await _stream.ReadAsync(_innerBuffer.AsMemory(_bufferLength))
                .ConfigureAwait(false);
            if (bytesRead == 0)
            {
                throw new SqlxException("Stream closed unexpectedly");
            }
            count -= bytesRead;
            _bufferLength += bytesRead;
        }
    }

    public async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        await FillBuffer(1).ConfigureAwait(false);
        return _innerBuffer[_bufferPosition++];
    }

    public async Task<int> ReadIntAsync(CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        await FillBuffer(4).ConfigureAwait(false);
        var result = new ReadBuffer(_innerBuffer.AsSpan(_bufferPosition))
            .ReadInt();
        _bufferPosition += 4;
        return result;
    }

    public async ValueTask ReadBuffer(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        await FillBuffer(buffer.Length).ConfigureAwait(false);
        _innerBuffer.AsMemory(_bufferPosition, buffer.Length).CopyTo(buffer);
        _bufferPosition += buffer.Length;
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

    private static async Task<IPEndPoint[]> GetIpEndpoints(
        string host,
        ushort port,
        CancellationToken cancellationToken)
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
        ArrayPool.Return(_innerBuffer);
        _innerBuffer = [];
    }
}
