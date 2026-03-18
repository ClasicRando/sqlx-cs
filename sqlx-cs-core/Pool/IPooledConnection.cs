namespace Sqlx.Core.Pool;

/// <summary>
/// Pooled connection object. Provides the necessary components to interact with a connection's
/// properties in an <see cref="AbstractConnectionPool{TConnection,TSelf}"/>.
/// </summary>
public interface IPooledConnection : IDisposable
{
    /// <summary>
    /// Unique ID of the connection
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Last timestamp at UTC when the connection was opened
    /// </summary>
    DateTime LastOpenTimestamp { get; }
    
    /// <summary>
    /// Current status of the connection
    /// </summary>
    ConnectionStatus Status { get; }
    
    /// <summary>
    /// Open the connection with the underlining database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    Task OpenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Check if the connection is truly valid. Sometimes <see cref="Status"/> might be stale so
    /// this method will perform a minimal operation with the database to confirm the underlining
    /// connection is still usable.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    /// <returns>True if the connection is still valid</returns>
    Task<bool> IsValidAsync(CancellationToken cancellationToken);

    void Cleanup();
}
