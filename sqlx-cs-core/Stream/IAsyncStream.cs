using System.IO.Pipelines;

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
    
    PipeWriter Writer { get; }
    
    PipeReader Reader { get; }
    
    /// <summary>
    /// Open the stream's connection to a remote host at the specified port
    /// </summary>
    /// <param name="host">host name/address to connect to</param>
    /// <param name="port">host port to connect to</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    Task OpenAsync(string host, ushort port, CancellationToken cancellationToken);
}
