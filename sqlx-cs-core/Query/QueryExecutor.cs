using System.Runtime.CompilerServices;
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

        /// <summary>
        /// Stream all rows returned from the query execution until the end of the first result set.
        /// Each row is mapped to <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map each row into</typeparam>
        /// <returns>a stream of result set rows mapped to the desired row type</returns>
        public async IAsyncEnumerable<TRow> FetchAsync<TRow>(
            string sql,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            var rows = query.FetchAsync<TDataRow, TRow>(cancellationToken);
            await foreach (TRow row in rows.ConfigureAwait(false))
            {
                yield return row;
            }
        }

        /// <summary>
        /// Fetch all rows returned from the query execution until the end of the first result set and
        /// pack all those rows into a <see cref="List{T}"/>. Each row is mapped to
        /// <typeparamref name="TRow"/> using the static constructor method <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>a list of result set rows mapped to the desired row type</returns>
        public async ValueTask<List<TRow>> FetchAllAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            return await query.FetchAllAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>the first row found when executing this query</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if zero rows are returned from the query
        /// </exception>
        public async Task<TRow> FetchFirstAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            return await query.FetchFirstAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>
        /// the first row found when executing this query or the default value when no rows are
        /// found
        /// </returns>
        public async Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            return await query.FetchFirstOrDefaultAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// thrown.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>the first row found when executing this query</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if zero or more than 1 row is returned
        /// </exception>
        public async Task<TRow> FetchSingleAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            return await query.FetchSingleAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>
        /// the first row found when executing this query or the default value when no rows are found
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if more than 1 row is returned
        /// </exception>
        public async Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            return await query.FetchSingleOrDefaultAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }
    }
    
    extension<TQuery, TBindable, TQueryBatch, TBindMany, TDataRow>(
        IQueryExecutor<TQuery, TBindable, TQueryBatch, TDataRow> queryExecutor)
        where TQuery : IExecutableQuery<TDataRow>, TBindable
        where TBindable : IBindable
        where TQueryBatch : IQueryBatch<TBindable, TDataRow>
        where TBindMany : IBindMany<TBindable>
        where TDataRow : IDataRow
    {
        /// <summary>
        /// Execute the supplied non-query using the parameters bound using <see cref="TBindMany"/>,
        /// ignoring any rows returned and just counting the total number of rows affected by the
        /// query. The intended use of this method is for queries that insert or manipulate data
        /// without returning any results.
        /// </summary>
        /// <param name="nonQuery">Command the returns no data and just modifies data</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">Token to cancel async operation</param>
        /// <returns>total number of rows impacted by the query</returns>
        public async Task<long> ExecuteNonQueryAsync(
            string nonQuery,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
        {
            using TQuery query = queryExecutor.CreateQuery(nonQuery);
            parameters.BindMany(query);
            return await query.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Stream all rows returned from the query execution until the end of the first result set.
        /// Each row is mapped to <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows with parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map each row into</typeparam>
        /// <returns>a stream of result set rows mapped to the desired row type</returns>
        public async IAsyncEnumerable<TRow> FetchAsync<TRow>(
            string sql,
            TBindMany parameters,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            parameters.BindMany(query);
            var rows = query.FetchAsync<TDataRow, TRow>(cancellationToken);
            await foreach (TRow row in rows.ConfigureAwait(false))
            {
                yield return row;
            }
        }

        /// <summary>
        /// Fetch all rows returned from the query execution until the end of the first result set and
        /// pack all those rows into a <see cref="List{T}"/>. Each row is mapped to
        /// <typeparamref name="TRow"/> using the static constructor method <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows with parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>a list of result set rows mapped to the desired row type</returns>
        public async ValueTask<List<TRow>> FetchAllAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            parameters.BindMany(query);
            return await query.FetchAllAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// </summary>
        /// <param name="sql">SQL query that fetches rows with parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>the first row found when executing this query</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if zero rows are returned from the query
        /// </exception>
        public async Task<TRow> FetchFirstAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            parameters.BindMany(query);
            return await query.FetchFirstAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows with parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>
        /// the first row found when executing this query or the default value when no rows are
        /// found
        /// </returns>
        public async Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            parameters.BindMany(query);
            return await query.FetchFirstOrDefaultAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// thrown.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows with parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>the first row found when executing this query</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if zero or more than 1 row is returned
        /// </exception>
        public async Task<TRow> FetchSingleAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            parameters.BindMany(query);
            return await query.FetchSingleAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows with parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>
        /// the first row found when executing this query or the default value when no rows are
        /// found
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if more than 1 row is returned
        /// </exception>
        public async Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<TDataRow, TRow>
        {
            using TQuery query = queryExecutor.CreateQuery(sql);
            parameters.BindMany(query);
            return await query.FetchSingleOrDefaultAsync<TDataRow, TRow>(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
