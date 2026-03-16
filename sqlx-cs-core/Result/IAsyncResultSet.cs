namespace Sqlx.Core.Result;

/// <summary>
/// <para>
/// Query result stream that represents zero or more results. Result sets are always zero or more
/// rows finalized with a <see cref="QueryResult"/>. This has a very similar signature to a
/// <see cref="IAsyncEnumerator{T}"/> because it essentially serves the same purpose but is specific
/// to result sets to avoid any divergent issues with the standard library interface.
/// </para>
/// <para>
/// Although this type exists and is available to use, in most cases there are extension methods for
/// <see cref="Sqlx.Core.Query.IExecutableQuery{T}"/> to execute the query and extract rows or rows
/// affected rather than interfacing with this type itself. However, for query batches where you are
/// retrieving multiple result sets, you cannot use those methods since they are not going to work.
/// In that case you can either:
/// <list type="bullet">
///     <item>use this interface directly to continue to extract query results</item>
///     <item>use this interface's extension methods to extract the known result sets</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// This must be disposed to allow for the underlining connection to be unlocked for future usage.
/// Row instances are valid until <see cref="MoveNextAsync"/> or <see cref="IDisposable.Dispose"/>
/// is called so use of an instance after the next result item is collected will throw an
/// <see cref="ObjectDisposedException"/>
/// </remarks>
/// <example>
/// <code>
/// using var asyncResult = await query.ExecuteQuery();
/// while (await asyncResultSet.MoveNextAsync())
/// {
///     var current = asyncResultSet.Current;
///     if (current.IsLeft)
///     {
///         var row = current.Left;
///         // Do something with results
///     }
///     else
///     {
///         var result = current.Right;
///         // Handle end of result
///     }
/// }
/// </code>
/// </example>
/// <typeparam name="TDataRow">Database specific row type</typeparam>
public interface IAsyncResultSet<TDataRow> : IDisposable where TDataRow : IDataRow
{
    /// <summary>
    /// Current result. Will always throw when <see cref="MoveNextAsync"/> has not been called. DO
    /// NOT hold reference to this after <see cref="MoveNextAsync"/> since row items are disposed
    /// of once a new item is fetched.
    /// </summary>
    Either<TDataRow, QueryResult> Current { get; }

    /// <summary>
    /// Move the next result item. Must be called before accessing <see cref="Current"/>
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel async operation</param>
    /// <returns>True if more items will be returned, otherwise false</returns>
    ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken = default);
}
