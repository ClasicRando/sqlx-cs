using System.Runtime.CompilerServices;
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

public class PgConnectionPool(PgConnectOptions options) : IConnectionPool
{
    public PgConnectOptions ConnectOptions { get; } = options;
    
    public Task<IConnection> Acquire(CancellationToken cancellationToken = default)
    {
        var stream = new PgStream(new AsyncStream(), ConnectOptions);
        return Task.FromResult<IConnection>(new PgConnection(stream, this));
    }

    public IExecutableQuery CreateQuery(string sql)
    {
        return new PgExecutableQuery(sql, this);
    }

    public IQueryBatch CreateQueryBatch()
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<Either<IDataRow, QueryResult>> ExecuteQuery(
        IQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using IConnection connection = await Acquire(cancellationToken);
        PgConnection pgConnection = PgException.CheckIfIs<IConnection, PgConnection>(connection);
        PgExecutableQuery executableQuery = PgException.CheckIfIs<IQuery, PgExecutableQuery>(query);
        await foreach (var result in pgConnection.ExecuteQuery(executableQuery, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return result;
        }
    }

    internal async Task<bool> GiveBack(PgConnection connection, CancellationToken cancellationToken)
    {
        connection.Pool = null;
        await connection.CloseAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
    
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
