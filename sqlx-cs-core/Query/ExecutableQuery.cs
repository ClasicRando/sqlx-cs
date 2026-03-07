using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Provides a set of extension methods for <see cref="IExecutableQuery{TDataRow}"/>
/// </summary>
public static class ExecutableQuery
{
    extension<TDataRow>(IExecutableQuery<TDataRow> executableQuery)
        where TDataRow : IDataRow
    {
        /// <summary>
        /// Execute this query, ignoring any rows returned and just counting the total number of
        /// rows affected by the query. The intended use of this method is for queries that insert
        /// or manipulate data without returning any results.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Total number of rows impacted by the query</returns>
        public async Task<long> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            using var resultSet = await executableQuery.ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            return await resultSet.CombineAllRowsAffected(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Stream all rows returned from the query execution until the end of the first result set.
        /// Each row is mapped to <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map each row into</typeparam>
        /// <returns>A stream of result set rows mapped to the desired row type</returns>
        public async IAsyncEnumerable<TRow> FetchAsync<TRow>(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using var resultSet = await executableQuery.ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            await foreach (TRow row in resultSet
                               .FetchNextResultAsync<TDataRow, TRow>(cancellationToken)
                               .ConfigureAwait(false))
            {
                yield return row;
            }
        }

        /// <summary>
        /// Fetch all rows returned from the query execution until the end of the first result set
        /// and pack all those rows into a <see cref="List{T}"/>. Each row is mapped to
        /// <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map the row into</typeparam>
        /// <returns>A list of result set rows mapped to the desired row type</returns>
        public async Task<List<TRow>> FetchAllAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using var resultSet = await executableQuery.ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            return await resultSet.ExtractNextResultAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map the row into</typeparam>
        /// <returns>The first row found when executing this query</returns>
        /// <exception cref="SqlxException">If zero rows are returned from the query</exception>
        public Task<TRow> FetchFirstAsync<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            return executableQuery.FetchRowAsync<TDataRow, TRow>(false, true, cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map the row into</typeparam>
        /// <returns>
        /// The first row found when executing this query or the default value when no rows are
        /// found
        /// </returns>
        public Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            return executableQuery.FetchRowAsync<TDataRow, TRow>(false, false, cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// thrown.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map the row into</typeparam>
        /// <returns>The first row found when executing this query</returns>
        /// <exception cref="SqlxException">If zero or more than 1 row is returned</exception>
        public Task<TRow> FetchSingleAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            return executableQuery.FetchRowAsync<TDataRow, TRow>(
                true,
                true,
                cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map the row into</typeparam>
        /// <returns>
        /// The first row found when executing this query or the default value when no rows are
        /// found
        /// </returns>
        /// <exception cref="SqlxException">If more than 1 row is returned</exception>
        public Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            return executableQuery.FetchRowAsync<TDataRow, TRow>(true, false, cancellationToken);
        }

        /// <summary>
        /// Internal method to fetch the first row of a query execution, handling first vs single
        /// and default vs required semantics.
        /// </summary>
        /// <param name="assumeSingleRow">True if only single row result sets are allowed</param>
        /// <param name="requireRow">True if at least 1 row is required</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TRow">Row type to map the row into</typeparam>
        /// <returns>The first row fetched or default is at least one row is not required</returns>
        /// <exception cref="SqlxException">
        /// If no rows are found and at least 1 is required or multiple rows are found and single
        /// row is expected
        /// </exception>
        private async Task<TRow?> FetchRowAsync<TRow>(
            bool assumeSingleRow,
            [DoesNotReturnIf(true)] bool requireRow,
            CancellationToken cancellationToken)
            where TRow : IFromRow<TDataRow, TRow>
        {
            var results = await executableQuery.FetchAsync<TDataRow, TRow>(cancellationToken)
                .Take(2)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (results.Count == 0)
            {
                return requireRow
                    ? throw new SqlxException("Expected at least 1 row but found 0")
                    : default;
            }

            if (assumeSingleRow && results.Count == 2)
            {
                throw new SqlxException("Expected a single row but found multiple");
            }

            return results[0];
        }
    }
}
