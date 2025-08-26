using Sqlx.Core.Query;

namespace Sqlx.Core.Connection;

public interface IConnection : IAsyncDisposable
{
    public bool IsConnected { get; }
    
    public bool InTransaction { get; }

    public Task OpenAsync(CancellationToken cancellationToken = default);

    public Task Begin(CancellationToken cancellationToken = default);

    public Task Commit(CancellationToken cancellationToken = default);

    public Task Rollback(CancellationToken cancellationToken = default);

    internal Task<bool> IsValid(CancellationToken cancellationToken = default);

    public IExecutableQuery CreateQuery(string sql);

    public IQueryBatch CreateQueryBatch();

    public Task CloseAsync(CancellationToken cancellationToken = default);
}
