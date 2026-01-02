using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Provides a set of extension methods for <see cref="IQueryBatch{TBindable, TDataRow}"/>
/// </summary>
public static class QueryBatch
{
    extension<TBindable, TDataRow>(IQueryBatch<TBindable, TDataRow> queryBatch)
        where TBindable : IBindable
        where TDataRow : IDataRow
    {
        /// <summary>
        /// Execute this query batch, ignoring any rows returned and just counting the total number
        /// of rows affected by the batch. The intended use of this method is for queries that
        /// insert or manipulate data without returning any results.
        /// </summary>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <returns>total number of rows impacted by the query batch</returns>
        public async Task<long> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            return await queryBatch.ExecuteBatch(cancellationToken)
                .OfType<Either<TDataRow, QueryResult>.Right>()
                .Select(result => result.Value.RowsAffected)
                .SumAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Wrap the <see cref="IQueryBatch{TBindable,TDataRow}"/> as a
        /// <see cref="QueryBatchResult{TBindable,TDataRow}"/> that allows for extracting each
        /// result set as it's respective rows. This method starts executes all steps leading up to
        /// the result stream being available so it may wait for that to complete depending upon the
        /// database's implementation of query batching.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the enumeration of results from the query batch execution. The
        /// <see cref="QueryBatchResult{TBindable,TDataRow}.ExtractNextResultAsync"/> method does
        /// not accept a <see cref="CancellationToken"/> since you cannot provide a new token after
        /// starting enumeration. If you want to control cancellation of the query batch extraction
        /// you must do so with this token.
        /// </param>
        /// <returns>Result set extraction wrapper class for a query batch</returns>
        public QueryBatchResult<TBindable, TDataRow> ToResult(
            CancellationToken cancellationToken = default)
        {
            return QueryBatchResult<TBindable, TDataRow>.Create(queryBatch, cancellationToken);
        }
    }
}
