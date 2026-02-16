using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Connector;

/// <summary>
/// Default implementation of <see cref="IAsyncConnector"/>
/// </summary>
[SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification =
        "Catches within this type should always be ignored since they indicate something has " +
        "gone very wrong but there is nothing we can do about it and the caller does not need to " +
        "be notified of the errors")]
public sealed class AsyncConnector : IAsyncConnector
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    private const int DefaultBufferSize = 1024 * 8;
    
    private Socket? _socket;
    private Stream? _stream;
    private PipeWriter? _pipeWriter;
    private bool _disposed;
    
    private byte[] _innerBuffer = ArrayPool.Rent(DefaultBufferSize);
    private int _bufferPosition;
    private int _bufferLength;

    public bool IsConnected => _socket?.Connected ?? false;

    public PipeWriter Writer => _pipeWriter ??
                                throw new InvalidOperationException(
                                    "Attempted to access the write buffer before opening the stream");

    public ReadOnlySpan<byte> ReadBuffer => _innerBuffer.AsSpan(_bufferPosition.._bufferLength);

    public async Task OpenAsync(string host, ushort port, CancellationToken cancellationToken)
    {
        var endPoints = await GetIpEndpointsAsync(host, port, cancellationToken)
            .ConfigureAwait(false);
        for (var i = 0; i < endPoints.Length; i++)
        {
            IPEndPoint ipEndPoint = endPoints[i];

            _socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await _socket.ConnectAsync(ipEndPoint, cancellationToken).ConfigureAwait(false);
                _stream = new NetworkStream(_socket);
                _pipeWriter = PipeWriter.Create(_stream);
            }
            catch (Exception e)
            {
                try
                {
                    _socket.Dispose();
                }
                catch
                {
                    // ignored
                }

                _socket = null;

                cancellationToken.ThrowIfCancellationRequested();

                if (i == endPoints.Length - 1)
                {
                    throw new SqlxException("Could not connect to host", e);
                }
            }
        }
    }

    private static async Task<IPEndPoint[]> GetIpEndpointsAsync(
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
    
    /// <summary>
    /// Fill the internal buffer up the desired length. If the buffer's size meets or exceeds the
    /// required length, no async operation is performed and the method exists early.
    /// </summary>
    /// <param name="length">require length of data in the internal buffer</param>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    /// <exception cref="SqlxException">if the stream is closed</exception>
    private async ValueTask FillBufferAsync(int length, CancellationToken cancellationToken)
    {
        var bytesRemaining = _bufferLength - _bufferPosition;
        if (bytesRemaining >= length)
        {
            return;
        }

        switch (length + bytesRemaining)
        {
            case > DefaultBufferSize:
            {
                ReallocateInternalBuffer(length + bytesRemaining);
                break;
            }
            case < DefaultBufferSize when _innerBuffer.Length > DefaultBufferSize:
            {
                ReallocateInternalBuffer(DefaultBufferSize);
                break;
            }
            default:
            {
                if (_bufferLength > 0)
                {
                    _innerBuffer.AsSpan()[_bufferPosition.._bufferLength]
                        .CopyTo(_innerBuffer);
                    _bufferLength = bytesRemaining;
                }

                break;
            }
        }

        _bufferPosition = 0;
        var count = length - bytesRemaining;
        while (count > 0)
        {
            var bytesRead = await _stream!.ReadAsync(
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

        return;

        void ReallocateInternalBuffer(int newSize)
        {
            var tempBuffer = _innerBuffer;
            _innerBuffer = ArrayPool.Rent(newSize);
            tempBuffer.AsSpan()[_bufferPosition.._bufferLength]
                .CopyTo(_innerBuffer);
            ArrayPool.Return(tempBuffer);
            _bufferLength = bytesRemaining;
        }
    }

    public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken)
    {
        CheckIfConnected();
        await FillBufferAsync(1, cancellationToken).ConfigureAwait(false);
        return _innerBuffer[_bufferPosition++];
    }

    public async ValueTask<int> ReadIntAsync(CancellationToken cancellationToken)
    {
        CheckIfConnected();
        await FillBufferAsync(4, cancellationToken).ConfigureAwait(false);
        ReadOnlySpan<byte> span = _innerBuffer.AsSpan(_bufferPosition);
        var result = span.ReadInt();
        _bufferPosition += 4;
        return result;
    }

    public ValueTask EnsureBufferFilled(int size, CancellationToken cancellationToken)
    {
        CheckIfConnected();
        return FillBufferAsync(size, cancellationToken);
    }

    public void AdvanceBufferPosition(int bytesConsumed)
    {
        _bufferPosition += bytesConsumed;
    }

    private void CheckIfConnected() => ThrowHelper.ThrowInvalidOperationExceptionIfNull(
        _stream,
        "Connection must be open to perform operation");

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _pipeWriter?.Complete();

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
        _pipeWriter = null;
    }
}
