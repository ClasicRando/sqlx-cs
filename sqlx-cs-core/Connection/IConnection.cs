using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Core.Connection;

/// <summary>
/// Database connection type. Provides the base ability to control transactional state and create
/// queries to execute against this connection.
/// </summary>
public interface
    IConnection<TQuery, out TBindable, TQueryBatch, TDataRow> :
    IQueryExecutor<TQuery, TBindable, TQueryBatch, TDataRow>,
    IAsyncDisposable
    where TQuery : IExecutableQuery<TDataRow>
    where TBindable : IBindable
    where TQueryBatch : IQueryBatch<TBindable, TDataRow>
    where TDataRow : IDataRow
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
    /// Open the connection to execute future queries. If this method is called by a connection that
    /// is already <see cref="ConnectionStatus.Idle"/>, then this method will not change the
    /// connection state. Otherwise, the physical connection will be closed and a new physical
    /// connection will be created.
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
    /// Close the connection
    /// </summary>
    /// <param name="cancellationToken">token to signal a cancellation</param>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
