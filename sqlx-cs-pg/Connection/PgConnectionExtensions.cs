using System.Runtime.CompilerServices;
using Sqlx.Core.Buffer;
using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

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
        /// Execute a <c>COPY TO</c> query against the database and forward the fetched rows to the
        /// supplied <see cref="Stream"/>.
        /// </summary>
        /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
        /// <param name="stream">
        /// Stream to forward data returned from the <c>COPY TO</c> command
        /// </param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task CopyOutAsync(
            ICopyTo copyOutStatement,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            var rows = pgConnection.CopyOutAsync(copyOutStatement, cancellationToken);
            await foreach (var row in rows.ConfigureAwait(false))
            {
                using var _ = row;
                await stream.WriteAsync(row.Memory, cancellationToken).ConfigureAwait(false);
            }
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
            // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
            var fileStream = new FileStream(path, fileMode);
            await using var _ = fileStream.ConfigureAwait(false);
            await pgConnection.CopyOutAsync(copyOutStatement, fileStream, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a <c>COPY {Table} TO BINARY</c> statement and collect the results into the
        /// desired row type. This is possible because the copy binary format is the same as rows
        /// sent during regular query execution and is easily mapped to a row type. 
        /// </summary>
        /// <param name="copyOutStatement">Binary copy out statement to execute</param>
        /// <param name="cancellationToken">Token to cancel async operation</param>
        /// <typeparam name="TRow">Row type to decode to</typeparam>
        /// <returns>Stream of rows from the copy statement</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the copy statement is not <see cref="ICopyQuery"/> or <see cref="ICopyTable"/>
        /// </exception>
        public async IAsyncEnumerable<TRow> CopyOutRowsAsync<TRow>(
            TableToBinary copyOutStatement,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            ArgumentNullException.ThrowIfNull(copyOutStatement);
            var columns = await pgConnection
                .QueryTableMetadataAsync(copyOutStatement, cancellationToken)
                .ConfigureAwait(false);
            var statementMetadata = new PgStatementMetadata(columns);

            var rows = pgConnection.CopyOutAsync(copyOutStatement, cancellationToken);

            var isFirstRow = true;
            await foreach (var row in rows.ConfigureAwait(false))
            {
                using var _ = row;
                var rowData = row.Memory.Span;
                // The first row will always be prefixed by 19 bytes of header data. We can just
                // skip that
                if (isFirstRow)
                {
                    isFirstRow = false;
                    rowData = rowData[19..];
                }
                
                // The final row will just be a -1 short value to indicate no more rows are present
                // so we skip that row. To ensure we complete the async enumeration we continue here
                // but the enumerable should not yield any more rows
                ReadOnlySpan<byte> span = rowData;
                if (span.ReadShort() == -1)
                {
                    continue;
                }

                ReadOnlySpan<byte> temp = rowData;
                using var dataRow = new PgDataRow(ref temp, statementMetadata);
                yield return TRow.FromRow(dataRow);
            }
        }

        private async ValueTask<PgColumnMetadata[]> QueryTableMetadataAsync(
            TableToBinary copyTable,
            CancellationToken cancellationToken)
        {
            using IPgExecutableQuery query = pgConnection.CreateQuery(CopyTableMetadata.Query);
            query.Bind(copyTable.TableName);
            query.Bind(copyTable.SchemaName);
            return await query.FetchAsync<CopyTableMetadata>(cancellationToken)
                .Select(m => m.GetColumnMetadata())
                .ToArrayAsync(cancellationToken)
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
            // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
            var fileStream = new FileStream(path, FileMode.Open);
            await using var _ = fileStream.ConfigureAwait(false);
            return await pgConnection.CopyInAsync(copyInStatement, fileStream, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private readonly record struct CopyTableMetadata(
        int TableOid,
        string ColumnName,
        short ColumnOrder,
        PgTypeInfo PgTypeInfo,
        short ColumnLength) : IFromRow<IPgDataRow, CopyTableMetadata>
    {
        public const string Query =
            """
            SELECT
                c.oid table_oid, attname AS column_name, attnum column_order, atttypid AS type_oid,
                attlen AS column_length
            FROM pg_attribute a
            JOIN pg_class c ON a.attrelid = c.oid
            JOIN pg_namespace n ON c.relnamespace = n.oid
            WHERE
                c.relname = $1
                AND n.nspname = $2
                AND a.attnum > 0
            """;

        public PgColumnMetadata GetColumnMetadata()
        {
            return new PgColumnMetadata(
                ColumnName,
                TableOid,
                ColumnOrder,
                PgTypeInfo,
                ColumnLength,
                0,
                PgFormatCode.Binary);
        }

        public static CopyTableMetadata FromRow(IPgDataRow dataRow)
        {
            return new CopyTableMetadata
            {
                TableOid = dataRow.GetIntNotNull("table_oid"),
                ColumnName = dataRow.GetStringNotNull("column_name"),
                ColumnOrder = dataRow.GetShortNotNull("column_order"),
                PgTypeInfo = PgTypeInfo.FromOid(dataRow.GetPgNotNull<PgOid>("type_oid")),
                ColumnLength = dataRow.GetShortNotNull("column_length"),
            };
        }
    }
}
