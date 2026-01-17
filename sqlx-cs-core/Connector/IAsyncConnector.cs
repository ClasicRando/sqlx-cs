using System.IO.Pipelines;

namespace Sqlx.Core.Connector;

/// <summary>
/// Base interface for async stream operations. Provides the basic read and write operations against
/// an underlining stream.
/// </summary>
public interface IAsyncConnector : IDisposable
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
    /// <param name="host">Host name/address to connect to</param>
    /// <param name="port">Host port to connect to</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    Task OpenAsync(string host, ushort port, CancellationToken cancellationToken);
}
