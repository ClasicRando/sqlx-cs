using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Sqlx.Core.Buffer;

namespace Sqlx.Core.Connector;

/// <summary>
/// <p>
/// Base interface for async stream operations. Provides the components to perform basic reading and
/// writing against the stream.
/// </p>
/// <h3>Reading</h3>
/// To read data from the stream, request a minimum number of bytes using
/// <see cref="EnsureBufferFilled"/> and then use <see cref="ReadBuffer"/> or
/// <see cref="ReadBufferMemory"/> to process the incoming data. After processing your required
/// data, the caller must finalize the read action by invoking <see cref="AdvanceBufferPosition"/>
/// which moves the read buffer forward past the consumed data. There is an underlining assumption
/// that the number of bytes needed is known but database protocols have sized messages so that
/// should always be true. Use extension methods in <see cref="BufferExtensions"/> to process the
/// read buffer data.
/// <h3>Writing</h3>
/// To write data to the stream, buffer data into <see cref="Writer"/> using the appropriate
/// <see cref="BufferExtensions"/> method for <see cref="IBufferWriter{byte}"/>s. Once all data is
/// written, call <see cref="PipeWriter.FlushAsync"/> to push all pending data to the stream.
/// </summary>
public interface IAsyncConnector : IDisposable
{
    /// <summary>
    /// True if the underlining stream is connected to the host
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Writer for the underlining stream
    /// </summary>
    PipeWriter Writer { get; }

    /// <summary>
    /// Read-only view of the read buffer. This span is only valid until a call to
    /// <see cref="EnsureBufferFilled"/> where the underlining buffer might become reset.
    /// </summary>
    ReadOnlySpan<byte> ReadBuffer { get; }

    /// <summary>
    /// Read-only view of the read buffer. This memory segment is only valid until a call to
    /// <see cref="EnsureBufferFilled"/> where the underlining buffer might become reset. Prefer
    /// using <see cref="ReadBuffer"/> unless you need a memory view that must exist past the scope
    /// that a ref struct can persist.
    /// </summary>
    ReadOnlyMemory<byte> ReadBufferMemory { get; }

    /// <summary>
    /// Open the stream's connection to a remote host at the specified port
    /// </summary>
    /// <param name="host">Host name/address to connect to</param>
    /// <param name="port">Host port to connect to</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    Task OpenAsync(string host, ushort port, CancellationToken cancellationToken);

    /// <summary>
    /// Fill the internal read buffer with at least the desired number of bytes. Returns immediately
    /// if the buffer already has that many bytes available.
    /// </summary>
    /// <param name="size">Number of bytes requested to be available in the read buffer</param>
    /// <param name="cancellationToken">Token to cancel the async action</param>
    /// <returns></returns>
    ValueTask EnsureBufferFilled(int size, CancellationToken cancellationToken);

    /// <summary>
    /// Move the internal read buffer forward the number of bytes that were consumed. This will make
    /// the data inaccessible upon future reads.
    /// </summary>
    /// <param name="bytesConsumed">Number of bytes consumed by a previous operation</param>
    void AdvanceBufferPosition(int bytesConsumed);
}

public static class AsyncConnectorExtensions
{
    extension(IAsyncConnector asyncConnector)
    {
        /// <summary>
        /// Read a single byte from the connection. Returns immediately if the internal read buffer
        /// is not empty.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the async action</param>
        /// <returns>Next available byte from the connection</returns>
        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            const int bytesNeeded = sizeof(byte);
            await asyncConnector.EnsureBufferFilled(bytesNeeded, cancellationToken)
                .ConfigureAwait(false);
            var span = asyncConnector.ReadBuffer;
            var result = span.ReadByte();
            asyncConnector.AdvanceBufferPosition(bytesNeeded);
            return result;
        }

        /// <summary>
        /// Read 4 bytes from the connection as an int. Returns immediately if the internal read
        /// buffer has enough bytes already.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the async action</param>
        /// <returns>Next available int from the connection</returns>
        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        public async ValueTask<int> ReadIntAsync(CancellationToken cancellationToken)
        {
            const int bytesNeeded = sizeof(int);
            await asyncConnector.EnsureBufferFilled(bytesNeeded, cancellationToken)
                .ConfigureAwait(false);
            var span = asyncConnector.ReadBuffer;
            var result = span.ReadInt();
            asyncConnector.AdvanceBufferPosition(bytesNeeded);
            return result;
        }
    }
}
