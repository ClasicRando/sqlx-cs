using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Interface for types that can execute a query to completion. This can either be done by the
/// object itself (e.g. a raw database connection) or objects that can defer to other objects that
/// can perform the execution (e.g. a connection the defers to a rented connection).
/// </summary>
public interface IQueryExecutor
{
    /// <summary>
    /// Execute the query and return an async stream of query result items
    /// </summary>
    /// <param name="query">query to execute</param>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    /// <returns>an async stream of query result items</returns>
    public Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteQuery(
        IQuery query,
        CancellationToken cancellationToken);
}
