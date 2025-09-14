using Sqlx.Core.Pool;
using Sqlx.Core.Query;

namespace Sqlx.Core.Connection;

/// <summary>
/// Database connection type. Provides the base ability to control transactional state and create
/// queries to execute against this connection.
/// </summary>
public interface IConnection : IQueryExecutor, IAsyncDisposable
{
    /// <summary>
    /// State of the connection. This is not directly connected to the state of the physical
    /// connection, but it reflects the lifecycle of the connection from the libraries' viewpoint.
    /// </summary>
    ConnectionStatus Status { get; }
    
    /// <summary>
    /// True if the connection is currently within a transaction
    /// </summary>
    bool InTransaction { get; }

    /// <summary>
    /// Open the connection to execute future queries. If this method is called upon an already open
    /// connection, that physical connection will be closed and a new physical connection will be
    /// created.
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start a new transaction against this connection. Calls to this method again before calling
    /// <see cref="CommitAsync"/> or <see cref="RollbackAsync"/> will result in an
    /// <see cref="Sqlx.Core.Exceptions.UnexpectedTransactionState"/> exception. Calls to
    /// <see cref="InTransaction"/> will return true until the transaction is closed.
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction of this connection. Calls to this method before calling
    /// <see cref="BeginAsync"/> will result in an
    /// <see cref="Sqlx.Core.Exceptions.UnexpectedTransactionState"/> exception. Calls to
    /// <see cref="InTransaction"/> will return false after this method exits.
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction of this connection. Calls to this method before calling
    /// <see cref="BeginAsync"/> will result in an
    /// <see cref="Sqlx.Core.Exceptions.UnexpectedTransactionState"/> exception. Calls to
    /// <see cref="InTransaction"/> will return false after this method exits.
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the current connection is valid. This method is intended for use by the internal
    /// components of the library so users should avoid using it. The assumption is that connections
    /// returned from a pool are already valid.
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    /// <returns>true if the connection is valid and usable, otherwise false</returns>
    Task<bool> IsValidAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Close the connection
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
