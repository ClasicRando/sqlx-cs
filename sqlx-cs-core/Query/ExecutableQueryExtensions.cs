using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public static class ExecutableQueryExtensions
{
    /// <summary>
    /// Execute this query, ignoring any rows returned and just counting the total number of rows
    /// affected by the query. The intended use of this method is for queries that insert or
    /// manipulate data without returning any results.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <returns>total number of rows impacted by the query</returns>
    public static async Task<long> ExecuteNonQuery(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
    {
        long count = 0;
        var results = await executableQuery.Execute(cancellationToken);
        await foreach (var result in results.WithCancellation(cancellationToken))
        {
            if (result is Either<IDataRow, QueryResult>.Right right)
            {
                count += right.Value.RowsAffected;
            }
        }

        return count;
    }

    /// <summary>
    /// Stream all rows returned from the query execution until the end of the first result set.
    /// Each row is mapped to <typeparamref name="TRow"/> using the static constructor method
    /// <see cref="TRow.FromRow"/>.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map each row into</typeparam>
    /// <returns>a stream of result set rows mapped to the desired row type</returns>
    public static async IAsyncEnumerable<TRow> Fetch<TRow>(
        this IExecutableQuery executableQuery,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        var results = await executableQuery.Execute(cancellationToken)
            .ConfigureAwait(false);
        await foreach (var result in results.ConfigureAwait(false)
                           .WithCancellation(cancellationToken))
        {
            switch (result)
            {
                case Either<IDataRow, QueryResult>.Right:
                    yield break;
                case Either<IDataRow, QueryResult>.Left left:
                    yield return TRow.FromRow(left.Value);
                    break;
            }
        }
    }

    /// <summary>
    /// Fetch all rows returned from the query execution until the end of the first result set and
    /// pack all those rows into a <see cref="List{T}"/>. Each row is mapped to
    /// <typeparamref name="TRow"/> using the static constructor method <see cref="TRow.FromRow"/>.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map the row into</typeparam>
    /// <returns>a list of result set rows mapped to the desired row type</returns>
    public static ValueTask<List<TRow>> FetchAll<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        return Fetch<TRow>(executableQuery, cancellationToken).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map the row into</typeparam>
    /// <returns>the first row found when executing this query</returns>
    /// <exception cref="SqlxException">if zero rows are returned from the query</exception>
    public static Task<TRow> FetchFirst<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        return FetchRow<TRow>(executableQuery, false, true, cancellationToken);
    }

    /// <summary>
    /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>. If
    /// no rows are returned by the query, <typeparamref name="TRow"/>'s default value is returned.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map the row into</typeparam>
    /// <returns>
    /// the first row found when executing this query or the default value when no rows are found
    /// </returns>
    public static Task<TRow?> FetchFirstOrDefault<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        return FetchRow<TRow>(executableQuery, false, false, cancellationToken);
    }

    /// <summary>
    /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
    /// thrown.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map the row into</typeparam>
    /// <returns>the first row found when executing this query</returns>
    /// <exception cref="SqlxException">if zero or more than 1 row is returned</exception>
    public static async Task<TRow> FetchSingle<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        return await FetchRow<TRow>(executableQuery, true, true, cancellationToken);
    }

    /// <summary>
    /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>. If
    /// no rows are returned by the query, <typeparamref name="TRow"/>'s default value is returned.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map the row into</typeparam>
    /// <returns>
    /// the first row found when executing this query or the default value when no rows are found
    /// </returns>
    /// <exception cref="SqlxException">if more than 1 row is returned</exception>
    public static Task<TRow?> FetchSingleOrDefault<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        return FetchRow<TRow>(executableQuery, true, false, cancellationToken);
    }

    /// <summary>
    /// Internal method to fetch the first row of a query execution, handling first vs single and
    /// default vs required semantics.
    /// </summary>
    /// <param name="executableQuery">query to execute</param>
    /// <param name="assumeSingleRow">true if only single row result sets are allowed</param>
    /// <param name="requireRow">true if at least 1 row is required</param>
    /// <param name="cancellationToken">optional cancellation token</param>
    /// <typeparam name="TRow">row type to map the row into</typeparam>
    /// <returns>the first row fetched or default is at least one row is not required</returns>
    /// <exception cref="SqlxException">
    /// if no rows are found and at least 1 is required or multiple rows are found and single row is
    /// expected
    /// </exception>
    private static async Task<TRow?> FetchRow<TRow>(
        this IExecutableQuery executableQuery,
        bool assumeSingleRow,
        [DoesNotReturnIf(true)] bool requireRow,
        CancellationToken cancellationToken)
        where TRow : IFromRow<TRow>
    {
        var results = Fetch<TRow>(executableQuery, cancellationToken).ConfigureAwait(false);
        await using ConfiguredCancelableAsyncEnumerable<TRow>.Enumerator enumerable = results.GetAsyncEnumerator();
        if (!await enumerable.MoveNextAsync())
        {
            if (requireRow)
            {
                throw new SqlxException("Expected at least 1 row but found 0");
            }
            return default;
        }

        TRow row = enumerable.Current;
        if (assumeSingleRow && await enumerable.MoveNextAsync())
        {
            throw new SqlxException("Expected a single row but found multiple");
        }

        return row;
    }
}
