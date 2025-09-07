using Sqlx.Core;
using Sqlx.Core.Connection;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Pool;

public sealed class PgConnectionPool(PgConnectOptions options) : IConnectionPool
{
    public PgConnectOptions ConnectOptions { get; } = options;
    
    public Task<IConnection> Acquire(CancellationToken cancellationToken = default)
    {
        var stream = new PgStream(new AsyncStream(), ConnectOptions);
        return Task.FromResult<IConnection>(new PgConnection(stream, this));
    }

    public IExecutableQuery CreateQuery(string query)
    {
        return new PgExecutableQuery(query, this);
    }

    public IQueryBatch CreateQueryBatch()
    {
        throw new NotImplementedException();
    }

    public async Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteQuery(
        IQuery query,
        CancellationToken cancellationToken)
    {
        await using var connection = await this.AcquireAs<PgConnection>(cancellationToken);
        PgExecutableQuery executableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return await connection.ExecuteQuery(executableQuery, cancellationToken);
    }

    public async Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteQueryBatch(IQueryBatch query, CancellationToken cancellationToken)
    {
        await using var connection = await this.AcquireAs<PgConnection>(cancellationToken);
        PgQueryBatch queryBatch = PgException.CheckIfIs<IQueryBatch, PgQueryBatch>(query);
        return await connection.ExecuteQueryBatch(queryBatch, cancellationToken);
    }

    internal async Task<bool> GiveBack(PgConnection connection, CancellationToken cancellationToken)
    {
        connection.Pool = null;
        await connection.ReleaseResources(cancellationToken).ConfigureAwait(false);
        return true;
    }
    
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
