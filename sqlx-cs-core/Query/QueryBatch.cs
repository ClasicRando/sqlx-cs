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
        public async Task<long> ExecuteNonQuery(CancellationToken cancellationToken = default)
        {
            long count = 0;
            var results = await queryBatch.ExecuteBatch(cancellationToken).ConfigureAwait(false);
            await foreach (var result in results.WithCancellation(cancellationToken))
            {
                if (result is Either<IDataRow, QueryResult>.Right right)
                {
                    count += right.Value.RowsAffected;
                }
            }

            return count;
        }
    }
}
