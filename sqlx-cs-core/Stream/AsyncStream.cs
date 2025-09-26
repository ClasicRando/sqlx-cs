using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Stream;

/// <summary>
/// Default implementation of <see cref="IAsyncStream"/>
/// </summary>
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
                    throw new IOException("Could not connect to host", e);
                }
            }
        }
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        return _stream.WriteAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Fill the internal buffer up the desired length. If the buffer's size meets or exceeds the
    /// required length, no async operation is performed and the method exists early.
    /// </summary>
    /// <param name="length">require length of data in the internal buffer</param>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    /// <exception cref="SqlxException">if the stream is closed</exception>
    private async ValueTask FillBuffer(int length, CancellationToken cancellationToken)
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
            var bytesRead = await _stream.ReadAsync(
                _innerBuffer.AsMemory(_bufferLength),
                cancellationToken)
                .ConfigureAwait(false);
            if (bytesRead == 0)
            {
                throw new IOException("Stream closed unexpectedly");
            }
            count -= bytesRead;
            _bufferLength += bytesRead;
        }
    }

    public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        await FillBuffer(1, cancellationToken).ConfigureAwait(false);
        return _innerBuffer[_bufferPosition++];
    }

    public async ValueTask<int> ReadIntAsync(CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        await FillBuffer(4, cancellationToken).ConfigureAwait(false);
        var result = new ReadBuffer(_innerBuffer.AsSpan(_bufferPosition))
            .ReadInt();
        _bufferPosition += 4;
        return result;
    }

    public async ValueTask ReadBuffer(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        SqlxException.ThrowIfNull(_stream);
        await FillBuffer(buffer.Length, cancellationToken).ConfigureAwait(false);
        _innerBuffer.AsMemory(_bufferPosition, buffer.Length).CopyTo(buffer);
        _bufferPosition += buffer.Length;
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

    public void Dispose()
    {
        if (_stream is SslStream sslStream)
        {
            try
            {
                sslStream.ShutdownAsync().GetAwaiter().GetResult();
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
            _stream?.Dispose();
        }
        catch
        {
            // ignored
        }
        
        if (_socket is not null && _socket.Connected)
        {
            try
            {
                _socket.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        _stream = null;
        _socket = null;
        _bufferLength = 0;
        _bufferPosition = 0;
        ArrayPool.Return(_innerBuffer);
        _innerBuffer = [];
    }
}
