using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Sqlx.Core.Result;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public static class PgConnectionExtensions
{
    extension(IPgConnection pgConnection)
    {
        /// <summary>
        /// Execute a <c>COPY TO</c> query against the database and forward the fetched rows to the
        /// supplied <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
        /// <param name="stream">
        /// Stream to forward data returned from the <c>COPY TO</c> command
        /// </param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task CopyOut(
            ICopyTo copyOutStatement,
            System.IO.Stream stream,
            CancellationToken cancellationToken = default)
        {
            var rows = await pgConnection.CopyOut(copyOutStatement, cancellationToken)
                .ConfigureAwait(false);
            await foreach (var row in rows.ConfigureAwait(false).WithCancellation(cancellationToken))
            {
                await stream.WriteAsync(row, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute a <c>COPY TO</c> query against the database and write all the returned data to
        /// the file path specified.
        /// </summary>
        /// <param name="copyOutStatement">COPY statement to execute for data extraction</param>
        /// <param name="path">File path to write the copy response data to</param>
        /// <param name="fileMode">File mode passed to the <see cref="FileStream"/></param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task CopyOut(
            ICopyTo copyOutStatement,
            string path,
            FileMode fileMode,
            CancellationToken cancellationToken = default)
        {
            await using var fileStream = new FileStream(path, fileMode);
            await pgConnection.CopyOut(copyOutStatement, fileStream, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a <c>COPY TO</c> statement and collect the results into the desired row type.
        /// This method expects a binary <c>COPY TO</c> statement since that format is the same as
        /// rows sent during regular query execution and is easily mapped 
        /// </summary>
        /// <param name="copyOutStatement">Binary copy out statement to execute</param>
        /// <param name="cancellationToken">Token to cancel async operation</param>
        /// <typeparam name="TCopyStatement">Copy statement type</typeparam>
        /// <typeparam name="TRow">Row type to decode to</typeparam>
        /// <returns>Stream of rows from the copy statement</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the copy statement is not <see cref="ICopyQuery"/> or <see cref="ICopyTable"/>
        /// </exception>
        public async IAsyncEnumerable<TRow> CopyOutRows<TCopyStatement, TRow>(
            TCopyStatement copyOutStatement,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TCopyStatement : ICopyTo, ICopyBinary
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            var columns = copyOutStatement switch
            {
                ICopyQuery copyQuery => await pgConnection
                    .FetchQueryMetadataAsync(copyQuery, cancellationToken)
                    .ConfigureAwait(false),
                ICopyTable copyTable => await pgConnection
                    .QueryTableMetadataAsync(copyTable, cancellationToken)
                    .ConfigureAwait(false),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(copyOutStatement),
                    copyOutStatement,
                    null),
            };
            var statementMetadata = new PgStatementMetadata(columns);

            var results = await pgConnection.CopyOut(copyOutStatement, cancellationToken)
                .ConfigureAwait(false);

            var isFirstRow = false;
            await foreach (var rowData in results.ConfigureAwait(false).WithCancellation(cancellationToken))
            {
                PgDataRow row;
                if (isFirstRow)
                {
                    isFirstRow = false;
                    row = new PgDataRow(rowData[19..], statementMetadata);
                }
                else
                {
                    row = new PgDataRow(rowData, statementMetadata);
                }

                yield return TRow.FromRow(row);
            }
        }

        private async Task<PgColumnMetadata[]> FetchQueryMetadataAsync(
            ICopyQuery copyQuery,
            CancellationToken cancellationToken)
        {
            PgConnection conn = PgException.CheckIfIs<IPgConnection, PgConnection>(pgConnection);
            PgPreparedStatement preparedStatement = await conn
                .GetOrPrepareStatement(copyQuery.Query, cancellationToken)
                .ConfigureAwait(false);
            return preparedStatement.ColumnMetadata;
        }

        private ValueTask<PgColumnMetadata[]> QueryTableMetadataAsync(
            ICopyTable copyTable,
            CancellationToken cancellationToken)
        {
            using IPgExecutableQuery query = pgConnection.CreateQuery(CopyTableMetadata.Query);
            query.Bind(copyTable.TableName);
            query.Bind(copyTable.SchemaName);
            return query.FetchAsync<CopyTableMetadata>(cancellationToken)
                .Select(m => m.GetColumnMetadata())
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Execute a <c>COPY FROM</c> query against the database and forward the data fetched from
        /// the <see cref="System.IO.Stream"/> as the copied data.
        /// </summary>
        /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
        /// <param name="stream">
        /// Stream to collect data for the <c>COPY FROM</c> command
        /// </param>
        /// <param name="pipeReaderOptions">
        /// Options supplied to <see cref="PipeReader.Create(Stream, StreamPipeReaderOptions)"/>
        /// </param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public Task<QueryResult> CopyIn(
            ICopyFrom copyInStatement,
            System.IO.Stream stream,
            StreamPipeReaderOptions? pipeReaderOptions = null,
            CancellationToken cancellationToken = default)
        {
            return pgConnection.CopyIn(
                copyInStatement,
                PipeReader.Create(stream, pipeReaderOptions),
                cancellationToken);
        }

        /// <summary>
        /// Execute a <c>COPY FROM</c> query against the database and copy all the data found at the
        /// specified path as the copied data
        /// </summary>
        /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
        /// <param name="path">File path that contains the data to copy to the database</param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task<QueryResult> CopyIn(
            ICopyFrom copyInStatement,
            string path,
            CancellationToken cancellationToken = default)
        {
            await using var fileStream = new FileStream(path, FileMode.Open);
            return await pgConnection.CopyIn(copyInStatement, fileStream, null, cancellationToken)
                .ConfigureAwait(false);
        }

        // /// <summary>
        // /// Execute a <c>COPY FROM</c> query against the database and copy all the data found at the
        // /// specified path as the copied data
        // /// </summary>
        // /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
        // /// <param name="cancellationToken">Token to cancel the async operation</param>
        // public async Task<QueryResult> CopyInRows<TCopyStatement, TCopyRow>(
        //     TCopyStatement copyInStatement,
        //     IAsyncEnumerable<IPgBinaryCopyRow> rows,
        //     CancellationToken cancellationToken = default)
        //     where TCopyStatement : ICopyFrom, ICopyBinary
        //     where TCopyRow : IPgBinaryCopyRow
        // {
        //     var pipe = new Pipe();
        //     Task dataStreamTask = Task.Run(() => WriteBinaryRowsToPipe(rows, pipe.Writer, cancellationToken), cancellationToken);
        //     var copyTask = pgConnection.CopyIn(copyInStatement, pipe.Reader, cancellationToken);
        //     await dataStreamTask;
        //     return await copyTask;
        // }
    }

    // private static ReadOnlySpan<byte> BinaryCopyHeader => [(byte)'P', (byte)'G', (byte)'C', (byte)'O', (byte)'P', (byte)'Y', 0x0A, 0xFF, 0x0D, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
    //
    // private static async Task WriteBinaryRowsToPipe(IAsyncEnumerable<IPgBinaryCopyRow> rows, PipeWriter writer, CancellationToken cancellationToken)
    // {
    //     await foreach (var row in rows.ConfigureAwait(false).WithCancellation(cancellationToken))
    //     {
    //     }
    // }

    internal readonly record struct CopyTableMetadata(
        int TableOid,
        string ColumnName,
        short ColumnOrder,
        PgTypeInfo PgTypeInfo,
        short ColumnLength) : IFromRow<IPgDataRow, CopyTableMetadata>
    {
        internal const string Query =
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
