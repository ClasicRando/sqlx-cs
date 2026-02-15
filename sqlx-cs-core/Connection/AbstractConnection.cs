using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Core.Connection;

public abstract class AbstractConnection<TQuery, TBindable, TQueryBatch, TDataRow> : IConnection<TQuery, TBindable, TQueryBatch, TDataRow>
    where TQuery : IExecutableQuery<TDataRow>
    where TBindable : IBindable
    where TQueryBatch : IQueryBatch<TBindable, TDataRow>
    where TDataRow : IDataRow
{
    ~AbstractConnection() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract ValueTask DisposeAsyncCore();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public abstract ConnectionStatus Status { get; }
    public abstract bool InTransaction { get; }

    public abstract Task OpenAsync(CancellationToken cancellationToken = default);

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommandAsync(TransactionCommand.Begin, cancellationToken);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommandAsync(TransactionCommand.Commit, cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteTransactionCommandAsync(TransactionCommand.Rollback, cancellationToken);
    }

    /// <summary>
    /// Execute the desired transaction command. If an error occurs trying to commiting a
    /// transaction, a <c>ROLLBACK</c> command will be tried as a last effort to resolve the
    /// transaction state, avoid locks and keep consistency.
    /// </summary>
    /// <param name="transactionCommand">Transaction command</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <exception cref="Sqlx.Core.Exceptions.UnexpectedTransactionState">
    /// If the transaction command would create an inconsistent state, such as attempting to:
    /// <list type="bullet">
    ///     <item>begin a new transaction while already within a transaction</item>
    ///     <item>commit or rollback a transaction while not within a transaction</item>
    /// </list>
    /// </exception>
    protected abstract Task ExecuteTransactionCommandAsync(
        TransactionCommand transactionCommand,
        CancellationToken cancellationToken);

    public abstract TQuery CreateQuery(string query);
    public abstract TQueryBatch CreateQueryBatch();

    public abstract Task<IAsyncResultSet<TDataRow>> ExecuteQueryAsync(
        TQuery query,
        CancellationToken cancellationToken);

    public abstract Task<IAsyncResultSet<TDataRow>> ExecuteQueryBatchAsync(
        TQueryBatch query,
        CancellationToken cancellationToken);
}
