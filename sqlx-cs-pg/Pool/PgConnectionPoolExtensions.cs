using System.Runtime.CompilerServices;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Pool;

public static class PgConnectionPoolExtensions
{
    extension(IPgConnectionPool connectionPool)
    {
        /// <summary>
        /// Acquire a connection from the pool and immediately start a new transaction against the
        /// connection before returning that rented connection. If starting a transaction fails, the
        /// underlining connection is returned to the pool.
        /// </summary>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <returns>a rented connection from the pool that is already within a transaction</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IPgConnection> BeginAsync(CancellationToken cancellationToken = default)
        {
            return connectionPool
                .BeginAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    IPgDataRow>(cancellationToken);
        }

        /// <summary>
        /// Execute the supplied non-query, ignoring any rows returned and just counting the total
        /// number of rows affected by the query. The intended use of this method is for queries
        /// that insert or manipulate data without returning any results.
        /// </summary>
        /// <param name="nonQuery">Command the returns no data and just modifies data</param>
        /// <param name="cancellationToken">Token to cancel async operation</param>
        /// <returns>total number of rows impacted by the query</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<long> ExecuteNonQueryAsync(
            string nonQuery,
            CancellationToken cancellationToken = default)
        {
            return connectionPool
                .ExecuteNonQueryAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    IPgDataRow>(nonQuery, cancellationToken);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerable<TRow> FetchAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    IPgDataRow, TRow>(sql, cancellationToken);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<List<TRow>> FetchAllAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchAllAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    IPgDataRow, TRow>(sql, cancellationToken);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchFirstAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchFirstAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    IPgDataRow, TRow>(sql, cancellationToken);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchFirstOrDefaultAsync<IPgConnection, IPgBindable, IPgExecutableQuery,
                    IPgQueryBatch, IPgDataRow, TRow>(sql, cancellationToken);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchSingleAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchSingleAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    IPgDataRow, TRow>(sql, cancellationToken);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchSingleOrDefaultAsync<IPgConnection, IPgBindable, IPgExecutableQuery,
                    IPgQueryBatch, IPgDataRow, TRow>(sql, cancellationToken);
        }
    }

    extension<TBindMany>(IPgConnectionPool connectionPool)
        where TBindMany : IBindMany<IPgBindable>
    {
        /// <summary>
        /// Execute the supplied non-query, ignoring any rows returned and just counting the total
        /// number of rows affected by the query. The intended use of this method is for queries
        /// that insert or manipulate data without returning any results.
        /// </summary>
        /// <param name="nonQuery">Command the returns no data and just modifies data</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">Token to cancel async operation</param>
        /// <returns>total number of rows impacted by the query</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<long> ExecuteNonQueryAsync(
            string nonQuery,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
        {
            return connectionPool
                .ExecuteNonQueryAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    TBindMany, IPgDataRow>(nonQuery, parameters, cancellationToken);
        }

        /// <summary>
        /// Stream all rows returned from the query execution until the end of the first result set.
        /// Each row is mapped to <typeparamref name="TRow"/> using the static constructor method
        /// <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map each row into</typeparam>
        /// <returns>a stream of result set rows mapped to the desired row type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerable<TRow> FetchAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch, TBindMany
                    , IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Fetch all rows returned from the query execution until the end of the first result set and
        /// pack all those rows into a <see cref="List{T}"/>. Each row is mapped to
        /// <typeparamref name="TRow"/> using the static constructor method <see cref="TRow.FromRow"/>.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>a list of result set rows mapped to the desired row type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<List<TRow>> FetchAllAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchAllAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    TBindMany, IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>the first row found when executing this query</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if zero rows are returned from the query
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchFirstAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchFirstAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    TBindMany, IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>
        /// the first row found when executing this query or the default value when no rows are
        /// found
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchFirstOrDefaultAsync<IPgConnection, IPgBindable, IPgExecutableQuery,
                    IPgQueryBatch, TBindMany, IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>
        /// thrown.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>the first row found when executing this query</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if zero or more than 1 row is returned
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchSingleAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchSingleAsync<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch,
                    TBindMany, IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Fetch the first row from the query execution and map it to <typeparamref name="TRow"/>.
        /// If no rows are returned by the query, <typeparamref name="TRow"/>'s default value is
        /// returned.
        /// </summary>
        /// <param name="sql">SQL query that fetches rows without parameters</param>
        /// <param name="parameters">Parameters bound to the query before execution</param>
        /// <param name="cancellationToken">optional cancellation token</param>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        /// <returns>
        /// the first row found when executing this query or the default value when no rows are found
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if more than 1 row is returned
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return connectionPool
                .FetchSingleOrDefaultAsync<IPgConnection, IPgBindable, IPgExecutableQuery,
                    IPgQueryBatch, TBindMany, IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }
    }
}
