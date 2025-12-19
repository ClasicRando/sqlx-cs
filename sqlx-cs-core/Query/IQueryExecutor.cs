using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Interface for types that can execute a query to completion. This can either be done by the
/// object itself (e.g. a raw database connection) or objects that can defer to other objects that
/// can perform the execution (e.g. a connection the defers to a rented connection).
/// </summary>
public interface IQueryExecutor<TQuery, out TBindable, TQueryBatch, TDataRow>
    where TQuery : IExecutableQuery<TDataRow>
    where TBindable : IBindable
    where TQueryBatch : IQueryBatch<TBindable, TDataRow>
    where TDataRow : IDataRow
{
    /// <summary>
    /// Create a new executable query the uses this connection to run the query. Make sure to keep
    /// this connection open until you complete the query execution and extract all results.
    /// </summary>
    /// <param name="query">Query to execute against the database</param>
    /// <returns>the executable query</returns>
    TQuery CreateQuery(string query);

    /// <summary>
    /// Create a new query batch the uses this connection to run the queries. Make sure to keep this
    /// connection open until you complete the query batch execution and extract all results.
    /// </summary>
    /// <returns>the query batch</returns>
    TQueryBatch CreateQueryBatch();
    
    /// <summary>
    /// Execute the query and return an async stream of query result items
    /// </summary>
    /// <param name="query">query to execute</param>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    /// <returns>an async stream of query result items</returns>
    Task<IAsyncEnumerable<Either<TDataRow, QueryResult>>> ExecuteQuery(
        TQuery query,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Execute the query batch and return an async stream of query result items
    /// </summary>
    /// <param name="query">query batch to execute</param>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    /// <returns>an async stream of query result items</returns>
    Task<IAsyncEnumerable<Either<TDataRow, QueryResult>>> ExecuteQueryBatch(
        TQueryBatch query,
        CancellationToken cancellationToken);
}
