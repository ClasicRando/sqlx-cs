using System.Runtime.CompilerServices;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Result;

public static class AsyncResultSet
{
    extension<TDataRow>(IAsyncResultSet<TDataRow> asyncResultSet) where TDataRow : IDataRow
    {
        /// <summary>
        /// Collect all results and add all <see cref="QueryResult.RowsAffected"/> to a
        /// <see cref="List{long}"/> to represent all rows affected by the executed query(s).
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel async operation</param>
        /// <returns>A list of rows impacted counts</returns>
        public async Task<List<long>> ExtractAllRowsAffected(
            CancellationToken cancellationToken = default)
        {
            List<long> result = [];
            while (await asyncResultSet.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var current = asyncResultSet.Current;
                if (current.IsRight)
                {
                    result.Add(current.Right.RowsAffected);
                }
            }

            return result;
        }
        
        /// <summary>
        /// Collect all results and sum the <see cref="QueryResult.RowsAffected"/> values returned
        /// from the database to represent all rows affected by the executed query(s).
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel async operation</param>
        /// <returns>
        /// A single value as the number of rows affected by the query(s) executed
        /// </returns>
        public async Task<long> CombineAllRowsAffected(
            CancellationToken cancellationToken = default)
        {
            var result = 0L;
            while (await asyncResultSet.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var current = asyncResultSet.Current;
                if (current.IsRight)
                {
                    result += current.Right.RowsAffected;
                }
            }

            return result;
        }
        
        /// <summary>
        /// Stream all rows until the end of the current query result. Each row is mapped to
        /// <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel async operation</param>
        /// <typeparam name="TRow">Row type to map each row into</typeparam>
        /// <returns>A stream of the next result rows mapped to the desired row type</returns>
        public async IAsyncEnumerable<TRow> FetchNextResultAsync<TRow>(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            while (await asyncResultSet.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var current = asyncResultSet.Current;
                if (current.IsLeft)
                {
                    yield return TRow.FromRow(current.Left);
                    continue;
                }

                yield break;
            }

            throw new InvalidOperationException(
                "Attempted to extract more results from a complete result set stream");
        }

        /// <summary>
        /// Extract all rows from the current query result. Each row is mapped to
        /// <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel async operation</param>
        /// <typeparam name="TRow">Row type to map each row into</typeparam>
        /// <returns>All rows from the current query result</returns>
        public async Task<List<TRow>> ExtractNextResultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            List<TRow> result = [];
            while (await asyncResultSet.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var current = asyncResultSet.Current;
                if (current.IsRight)
                {
                    return result;
                }

                result.Add(TRow.FromRow(current.Left));
            }

            throw new InvalidOperationException(
                "Attempted to extract more results from a complete result set stream");
        }
    }
}
