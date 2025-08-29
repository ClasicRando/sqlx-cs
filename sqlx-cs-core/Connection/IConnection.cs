using Sqlx.Core.Exceptions;
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

    public Task<bool> IsValid(CancellationToken cancellationToken = default);

    public IExecutableQuery CreateQuery(string sql);

    public IQueryBatch CreateQueryBatch();

    public TConnection Unwrap<TConnection>() where TConnection : IConnection
    {
        if (this is TConnection result)
        {
            return result;
        }

        throw new SqlxException($"Could not unwrap a {GetType()} as {typeof(TConnection)}");
    }

    public Task CloseAsync(CancellationToken cancellationToken = default);
}
