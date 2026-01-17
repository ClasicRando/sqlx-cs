using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Sqlx.Core.Buffer;
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
                await stream.WriteAsync(row, cancellationToken).ConfigureAwait(false);
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
            FileMode fileMode = FileMode.Open,
            CancellationToken cancellationToken = default)
        {
            // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
            var fileStream = new FileStream(path, fileMode);
            await using var _ = fileStream.ConfigureAwait(false);
            await pgConnection.CopyOutAsync(copyOutStatement, fileStream, cancellationToken)
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
        public async IAsyncEnumerable<TRow> CopyOutRowsAsync<TCopyStatement, TRow>(
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

            var rows = pgConnection.CopyOutAsync(copyOutStatement, cancellationToken);

            var isFirstRow = true;
            await foreach (var row in rows.ConfigureAwait(false))
            {
                var rowData = row;
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
                ReadOnlySpan<byte> span = rowData.AsSpan();
                if (span.ReadShort() == -1)
                {
                    continue;
                }
                
                var dataRow = new PgDataRow(rowData, statementMetadata);
                yield return TRow.FromRow(dataRow);
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
        /// the <see cref="Stream"/> as the copied data.
        /// </summary>
        /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
        /// <param name="stream">
        /// Stream to collect data for the <c>COPY FROM</c> command
        /// </param>
        /// <param name="pipeReaderOptions">
        /// Options supplied to <see cref="PipeReader.Create(Stream, StreamPipeReaderOptions)"/>
        /// </param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public Task<QueryResult> CopyInAsync(
            ICopyFrom copyInStatement,
            Stream stream,
            StreamPipeReaderOptions? pipeReaderOptions = null,
            CancellationToken cancellationToken = default)
        {
            return pgConnection.CopyInAsync(
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
        public async Task<QueryResult> CopyInAsync(
            ICopyFrom copyInStatement,
            string path,
            CancellationToken cancellationToken = default)
        {
            // This is a workaround for calling ConfigureAwait on an IAsyncDisposable
            var fileStream = new FileStream(path, FileMode.Open);
            await using var _ = fileStream.ConfigureAwait(false);
            return await pgConnection.CopyInAsync(copyInStatement, fileStream, null, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a <c>COPY FROM</c> query against the database and copies all supplied
        /// <paramref name="rows"/> as the copy data.  
        /// </summary>
        /// <param name="copyInStatement">COPY statement to execute for data extraction</param>
        /// <param name="rows">
        /// Async stream of copy rows to encode and send to the server as the copy statement row
        /// data
        /// </param>
        /// <param name="cancellationToken">Token to cancel the async operation</param>
        public async Task<QueryResult> CopyInRowsAsync<TCopyStatement, TCopyRow>(
            TCopyStatement copyInStatement,
            IAsyncEnumerable<TCopyRow> rows,
            CancellationToken cancellationToken = default)
            where TCopyStatement : ICopyFrom, ICopyBinary
            where TCopyRow : IPgBinaryCopyRow
        {
            var pipe = new Pipe();
            Task dataStreamTask = Task.Run(
                () => WriteBinaryRowsToPipe(rows, pipe.Writer, cancellationToken),
                cancellationToken);
            var copyTask = pgConnection.CopyInAsync(copyInStatement, pipe.Reader, cancellationToken);
            await dataStreamTask.ConfigureAwait(false);
            return await copyTask.ConfigureAwait(false);
        }
    }

    private const int MaxCopyBufferSize = 8192;

    private static readonly byte[] BinaryCopyHeader =
    [
        (byte)'P', (byte)'G', (byte)'C', (byte)'O', (byte)'P', (byte)'Y', 0x0A, 0xFF, 0x0D, 0x0A,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    private static async Task WriteBinaryRowsToPipe<TCopyRow>(
        IAsyncEnumerable<TCopyRow> rows,
        PipeWriter writer,
        CancellationToken cancellationToken)
        where TCopyRow : IPgBinaryCopyRow
    {
        using PooledArrayBufferWriter buffer = new();
        writer.Write(BinaryCopyHeader);
        await foreach (TCopyRow row in rows.ConfigureAwait(false)
                           .WithCancellation(cancellationToken))
        {
            using PgParameterWriter parameterWriter = new(buffer);
            buffer.WriteShort(TCopyRow.ColumnCount);
            row.BindValues(parameterWriter);

            if (buffer.WrittenCount < MaxCopyBufferSize)
            {
                continue;
            }

            writer.Write(buffer.ReadableSpan);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            buffer.Reset();
        }

        if (buffer.WrittenCount > 0)
        {
            writer.Write(buffer.ReadableSpan);
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        await writer.CompleteAsync().ConfigureAwait(false);
    }

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
