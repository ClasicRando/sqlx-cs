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
    
    public IConnection CreateConnection()
    {
        return new PgConnection(this);
    }

    internal Task<PgStream> AcquireStream()
    {
        return Task.FromResult(new PgStream(new AsyncStream(), this));
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
        await using var connection = this.CreateConnectionAs<PgConnection>();
        PgExecutableQuery executableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        return await connection.ExecuteQuery(executableQuery, cancellationToken);
    }

    public async Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteQueryBatch(IQueryBatch query, CancellationToken cancellationToken)
    {
        await using var connection = this.CreateConnectionAs<PgConnection>();
        PgQueryBatch queryBatch = PgException.CheckIfIs<IQueryBatch, PgQueryBatch>(query);
        return await connection.ExecuteQueryBatch(queryBatch, cancellationToken);
    }

    internal async ValueTask Return(PgStream stream, CancellationToken cancellationToken)
    {
        await stream.CleanUp(cancellationToken);
        stream.Dispose();
    }
    
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
