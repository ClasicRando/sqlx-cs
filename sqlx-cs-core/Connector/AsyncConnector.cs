using System.Buffers;
using System.Diagnostics.CodeAnalysis;
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
public sealed class AsyncConnector(int readBufferSize, int writeBufferSize) : IAsyncConnector
{
    private Socket? _socket;
    private Stream? _stream;
    private bool _disposed;

    private readonly ArrayBufferReader _readBuffer = new(readBufferSize);
    private readonly ArrayBufferWriter _writeBuffer = new(
        initialCapacity: writeBufferSize,
        usePooledArray: false);

    public bool IsConnected => _socket?.Connected ?? false;

    public IBufferWriter<byte> Writer => _writeBuffer;

    public IBufferReader Reader => _readBuffer;

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
                    throw new IOException("Could not connect to host", e);
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

    public ValueTask EnsureReadBufferFilled(int size, CancellationToken cancellationToken)
    {
        CheckIfConnected();
        return _readBuffer.FillBufferAsync(_stream!, size, cancellationToken);
    }

    public async ValueTask FlushWriteBuffer(CancellationToken cancellationToken)
    {
        CheckIfConnected();
        await _stream!.WriteAsync(_writeBuffer.ReadableMemory, cancellationToken)
            .ConfigureAwait(false);
        _writeBuffer.Clear();
    }

    public void ResetBuffers()
    {
        _readBuffer.ResetToInitialCapacity();
        _writeBuffer.ResetToInitialCapacity();
    }

    private void CheckIfConnected() => ThrowHelper.ThrowInvalidOperationExceptionIfNull(
        _stream,
        "Connection must be open to perform operation");

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

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
        _readBuffer.Dispose();
        _writeBuffer.Dispose();
    }
}
