using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Stream;

/// <summary>
/// Default implementation of <see cref="IAsyncStream"/>
/// </summary>
public sealed class AsyncStream : IAsyncStream
{
    private Socket? _socket;
    private System.IO.Stream? _stream;
    private PipeWriter? _pipeWriter;
    private PipeReader? _pipeReader;
    private bool _disposed;

    public bool IsConnected => _socket?.Connected ?? false;

    public PipeWriter Writer => _pipeWriter ?? throw new SqlxException("Attempted to access the write buffer before opening the stream");

    public PipeReader Reader => _pipeReader ?? throw new SqlxException("Attempted to access the write buffer before opening the stream");

    public async Task OpenAsync(string host, ushort port, CancellationToken cancellationToken)
    {
        var endPoints = await GetIpEndpointsAsync(host, port, cancellationToken);
        for (var i = 0; i < endPoints.Length; i++)
        {
            IPEndPoint ipEndPoint = endPoints[i];

            var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(ipEndPoint, cancellationToken);
                _socket = socket;
                _stream = new NetworkStream(_socket);
                _pipeWriter = PipeWriter.Create(_stream);
                _pipeReader = PipeReader.Create(_stream);
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

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _pipeWriter?.Complete();
        _pipeReader?.Complete();
        
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
        _pipeReader = null;
    }
}
