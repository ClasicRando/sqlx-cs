using System.Runtime.CompilerServices;
using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Connection;

public static class PgConnectionExtensions
{
    extension(IPgConnection pgConnection)
    {
        /// <inheritdoc cref="ConnectionExtensions.ExecuteNonQueryAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<long> ExecuteNonQueryAsync(
            string nonQuery,
            CancellationToken cancellationToken = default)
        {
            return pgConnection
                .ExecuteNonQueryAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>(
                    nonQuery,
                    cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow,TRow}"/>
        public IAsyncEnumerable<TRow> FetchAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow, TRow>(
                    sql,
                    cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchAllAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow,TRow}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<List<TRow>> FetchAllAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchAllAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow, TRow>(
                    sql,
                    cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchFirstAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow,TRow}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchFirstAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchFirstAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow, TRow>(
                    sql,
                    cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchFirstOrDefaultAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow,TRow}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchFirstOrDefaultAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow
                    , TRow>(sql, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchSingleAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow,TRow}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchSingleAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchSingleAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow, TRow>(
                    sql,
                    cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchSingleOrDefaultAsync{TExecutableQuery,TBindable,TQueryBatch,TDataRow,TRow}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            string sql,
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchSingleOrDefaultAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch,
                    IPgDataRow, TRow>(sql, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.ExecuteNonQueryAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<long> ExecuteNonQueryBatchAsync<TBindMany>(
            string nonQuery,
            IEnumerable<TBindMany> parameters,
            bool wrapBatchInTransaction = false,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
        {
            return pgConnection
                .ExecuteNonQueryBatchAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany
                    , IPgDataRow>(nonQuery, parameters, wrapBatchInTransaction, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.ExecuteNonQueryAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<long> ExecuteNonQueryAsync<TBindMany>(
            string nonQuery,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
        {
            return pgConnection
                .ExecuteNonQueryAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany,
                    IPgDataRow>(nonQuery, parameters, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow,TRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerable<TRow> FetchAsync<TBindMany, TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany, IPgDataRow,
                    TRow>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow,TRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<List<TRow>> FetchAllAsync<TBindMany, TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchAllAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany, IPgDataRow
                    , TRow>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchFirstAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow,TRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchFirstAsync<TBindMany, TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchFirstAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany,
                    IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchFirstOrDefaultAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow,TRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchFirstOrDefaultAsync<TBindMany, TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchFirstOrDefaultAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany,
                    IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchSingleAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow,TRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchSingleAsync<TBindMany, TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchSingleAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany,
                    IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <inheritdoc cref="ConnectionExtensions.FetchSingleOrDefaultAsync{TExecutableQuery,TBindable,TQueryBatch,TBindMany,TDataRow,TRow}"/>
        /// <typeparam name="TBindMany"></typeparam>
        /// <typeparam name="TRow">row type to map the row into</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchSingleOrDefaultAsync<TBindMany, TRow>(
            string sql,
            TBindMany parameters,
            CancellationToken cancellationToken = default)
            where TBindMany : IBindMany<IPgBindable>
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return pgConnection
                .FetchSingleOrDefaultAsync<IPgExecutableQuery, IPgBindable, IPgQueryBatch, TBindMany
                    , IPgDataRow, TRow>(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Execute a <c>COPY TO</c> query against the database and write all the returned data to
        /// the file path specified.
        /// </summary>
        /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
        /// <param name="path">File path to write the copy response data to</param>
        /// <param name="fileMode">
        /// Optional <see cref="FileMode"/> passed to the <see cref="FileStream"/>. Defaults to
        /// <see cref="FileMode.Open"/>.
        /// </param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task CopyOutAsync(
            ICopyTo copyOutStatement,
            string path,
            FileMode fileMode = FileMode.OpenOrCreate,
            CancellationToken cancellationToken = default)
        {
            var fileStream = new FileStream(path, fileMode);
            // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
            await using var _ = fileStream.ConfigureAwait(false);
            await pgConnection.CopyOutAsync(copyOutStatement, fileStream, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a <c>COPY FROM</c> query against the database and copy all the data found at the
        /// specified path as the copied data
        /// </summary>
        /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
        /// <param name="path">File path that contains the data to copy to the database</param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task<QueryResult> CopyInAsync(
            ICopyFrom copyInStatement,
            string path,
            CancellationToken cancellationToken = default)
        {
            var fileStream = new FileStream(path, FileMode.Open);
            // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
            await using var _ = fileStream.ConfigureAwait(false);
            return await pgConnection.CopyInAsync(copyInStatement, fileStream, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
