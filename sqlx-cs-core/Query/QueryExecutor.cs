using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public static class QueryExecutor
{
    extension<TQuery, TBindable, TQueryBatch, TDataRow>(
        IQueryExecutor<TQuery, TBindable, TQueryBatch, TDataRow> queryExecutor)
        where TQuery : IExecutableQuery<TDataRow>
        where TBindable : IBindable
        where TQueryBatch : IQueryBatch<TBindable, TDataRow>
        where TDataRow : IDataRow
    {
        /// <summary>
        /// Execute the supplied non-query, ignoring any rows returned and just counting the total
        /// number of rows affected by the query. The intended use of this method is for queries
        /// that insert or manipulate data without returning any results.
        /// </summary>
        /// <param name="nonQuery">Command the returns no data and just modifies data</param>
        /// <param name="cancellationToken">Token to cancel async operation</param>
        /// <returns>total number of rows impacted by the query</returns>
        public async Task<long> ExecuteNonQueryAsync(
            string nonQuery,
            CancellationToken cancellationToken = default)
        {
            using TQuery query = queryExecutor.CreateQuery(nonQuery);
            return await query.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
