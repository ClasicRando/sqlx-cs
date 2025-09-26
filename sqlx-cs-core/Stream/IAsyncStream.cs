namespace Sqlx.Core.Stream;

/// <summary>
/// Base interface for async stream operations. Provides the basic read and write operations against
/// an underlining stream.
/// </summary>
public interface IAsyncStream : IDisposable
{
    /// <summary>
    /// True if the underlining stream is connected to the host
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Open the stream's connection to a remote host at the specified port
    /// </summary>
    /// <param name="host">host name/address to connect to</param>
    /// <param name="port">host port to connect to</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task OpenAsync(string host, ushort port, CancellationToken cancellationToken);
    
    /// <summary>
    /// Write the entire memory segment to the stream
    /// </summary>
    /// <param name="buffer">memory segment to write to the stream</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

    /// <summary>
    /// Read a single byte from the stream
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>the next byte from the stream</returns>
    ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Read the next 4 bytes from the stream and combine into a single int
    /// </summary>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>the next integer from the stream</returns>
    ValueTask<int> ReadIntAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Read as many bytes as requested by the supplied buffer and copy those bytes into the buffer
    /// </summary>
    /// <param name="buffer">buffer to copy bytes into</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    ValueTask ReadBuffer(Memory<byte> buffer, CancellationToken cancellationToken);
}
