using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Super interface of <see cref="IQuery"/> that allows for executing the query.
/// </summary>
public interface IExecutableQuery : IQuery
{
    /// <summary>
    /// <para>
    /// Executes the query and returns an async generator of rows and query results. During normal
    /// query execution the database will return zero or more rows finalized with a query result
    /// message. This stream of data represents that same stream of messages. This stream also
    /// supports batch results where more than 1 result set is returned from the server.
    /// </para>
    /// <para>
    /// This is considered a low-level API for query execution since the user needs to manually
    /// process the rows and results. Prefer extension methods such as:
    /// <list type="bullet">
    ///     <item><see cref="ExecutableQueryExtensions.ExecuteNonQuery"/></item>
    ///     <item><see cref="ExecutableQueryExtensions.Fetch{T}"/></item>
    ///     <item><see cref="ExecutableQueryExtensions.FetchFirst{T}"/></item>
    ///     <item><see cref="ExecutableQueryExtensions.FetchFirstOrDefault{T}"/></item>
    ///     <item><see cref="ExecutableQueryExtensions.FetchSingle{T}"/></item>
    ///     <item><see cref="ExecutableQueryExtensions.FetchSingleOrDefault{T}"/></item>
    ///     <item><see cref="ExecutableQueryExtensions.FetchAll{T}"/></item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>async generator of query result objects</returns>
    public Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> Execute(
        CancellationToken cancellationToken);
}
