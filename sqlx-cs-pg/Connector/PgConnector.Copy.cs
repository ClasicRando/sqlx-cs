using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Sqlx.Core.Buffer;
using Sqlx.Core.Config;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Connector;

public sealed partial class PgConnector
{
    private const int MaxCopyDataSendSize = 8192;

    /// <summary>
    /// Execute a <c>COPY TO</c> statement against this connection. Initiates the copy operation
    /// followed by yielding each data row until the query execution is complete
    /// </summary>
    /// <param name="copyOutStatement">Copy to statement</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Stream of data rows as raw bytes</returns>
    internal async IAsyncEnumerable<byte[]> CopyOut(
        ICopyTo copyOutStatement,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ThrowIfNotOpen();

        using UserAction _ = StartUserAction();
        
        await WaitUntilReady(cancellationToken).ConfigureAwait(false);
        await SendQueryMessage(copyOutStatement.ToCopyQuery(), cancellationToken)
            .ConfigureAwait(false);
        CopyOutResponseMessage message = await WaitForOrThrowError<CopyOutResponseMessage>(cancellationToken)
            .ConfigureAwait(false);
        _pendingReadyForQuery++;
        _logger.LogCopyOutResponse(message);

        Status = ConnectionStatus.Fetching;
        while (true)
        {
            PgBackendMessageType backendMessageType =
                await ReceiveNextMessageType(cancellationToken)
                    .ConfigureAwait(false);
            var size = await ReceiveNextMessageSize(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            switch (backendMessageType)
            {
                case PgBackendMessageType.CopyData:
                    var data = ReceiveCopyDataMessage(size);
                    yield return data;
                    break;
                case PgBackendMessageType.CopyDone:
                case PgBackendMessageType.CommandComplete:
                    AdvanceReadBuffer(size);
                    break;
                case PgBackendMessageType.ReadyForQuery:
                    HandleReadyForQueryMessage(size);
                    yield break;
                default:
                    AdvanceReadBuffer(size);
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessageType);
                    break;
            }
        }
    }

    /// <summary>
    /// Executes a <c>COPY FROM</c> statement against this connection. Initiates the copy operation
    /// followed by forwarding all data received from the <see cref="PipeReader"/> to the database
    /// as COPY data messages. 
    /// </summary>
    /// <param name="copyInStatement">Copy from statement</param>
    /// <param name="data">Data pipe containing all user supplied data</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Query result data</returns>
    internal async Task<QueryResult> CopyIn(
        ICopyFrom copyInStatement,
        Stream data,
        CancellationToken cancellationToken)
    {
        ThrowIfNotOpen();

        using UserAction _ = StartUserAction();
        await WaitUntilReady(cancellationToken).ConfigureAwait(false);
        await SendQueryMessage(copyInStatement.ToCopyQuery(), cancellationToken)
            .ConfigureAwait(false);
        CopyInResponseMessage message = await WaitForOrThrowError<CopyInResponseMessage>(cancellationToken)
            .ConfigureAwait(false);
        _pendingReadyForQuery++;
        _logger.LogCopyInResponse(message);

        try
        {
            await SendCopyDataAsync(data, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await SendCopyFailAsync(cancellationToken: cancellationToken, failCause: e)
                .ConfigureAwait(false);
            throw;
        }

        return await CollectCopyInResult(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Forward all data from the pipe to the internal buffer (as <c>COPY DATA</c> messages),
    /// flushing when the internal buffer is full and finalizing the operation with a
    /// <c>COPY DONE</c> message.
    /// </summary> 
    /// <param name="data">Data source as pipe</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <exception cref="PgException">
    /// If the pipe reader is cancelled before it's completed
    /// </exception>
    private async Task SendCopyDataAsync(Stream data, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
        try
        {
            var bytesWritten = 0;
            int length;
            while ((length = await data.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
            {
                if (bytesWritten > MaxCopyDataSendSize)
                {
                    await FlushStream(cancellationToken).ConfigureAwait(false);
                }

                WriteCopyDataMessage(buffer.AsSpan(0, length));
                bytesWritten += length;
            }
            await FlushStream(cancellationToken).ConfigureAwait(false);
            await SendCopyDoneMessage(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Executes a <c>COPY FROM</c> statement against this connection. Initiates the copy operation
    /// followed by forwarding all data rows encoded in binary format to the database.
    /// </summary>
    /// <param name="copyInStatement">Copy from statement</param>
    /// <param name="rows">Stream of rows all user supplied data</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Query result data</returns>
    internal async Task<QueryResult> CopyIn<TCopyStatement, TCopyRow>(
        TCopyStatement copyInStatement,
        IAsyncEnumerable<TCopyRow> rows,
        CancellationToken cancellationToken)
        where TCopyStatement : ICopyFrom, ICopyBinary
        where TCopyRow : IPgBinaryCopyRow
    {
        ThrowIfNotOpen();

        using UserAction _ = StartUserAction();
        await WaitUntilReady(cancellationToken).ConfigureAwait(false);
        await SendQueryMessage(copyInStatement.ToCopyQuery(), cancellationToken)
            .ConfigureAwait(false);
        CopyInResponseMessage message = await WaitForOrThrowError<CopyInResponseMessage>(cancellationToken)
            .ConfigureAwait(false);
        _pendingReadyForQuery++;
        _logger.LogCopyInResponse(message);

        try
        {
            await SendCopyRowsAsync(rows, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await SendCopyFailAsync(cancellationToken: cancellationToken, failCause: e)
                .ConfigureAwait(false);
            throw;
        }

        return await CollectCopyInResult(cancellationToken)
            .ConfigureAwait(false);
    }

    private const int MaxCopyBufferSize = 8192;

    private static readonly byte[] BinaryCopyHeader =
    [
        (byte)'P',(byte)'G',(byte)'C',(byte)'O',(byte)'P',(byte)'Y',
        (byte)'\n', 0xFF, (byte)'\r', (byte)'\n', 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
    ];

    private static readonly byte[] BinaryCopyFooter = [0xFF, 0xFF];

    /// <summary>
    /// Forward all data from the pipe to the internal buffer (as <c>COPY DATA</c> messages),
    /// flushing when the internal buffer is full and finalizing the operation with a
    /// <c>COPY DONE</c> message.
    /// </summary> 
    /// <param name="rows">Data source as a stream of rows</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <exception cref="PgException">
    /// If the pipe reader is cancelled before it's completed
    /// </exception>
    private async Task SendCopyRowsAsync<TCopyRow>(
        IAsyncEnumerable<TCopyRow> rows,
        CancellationToken cancellationToken)
        where TCopyRow : IPgBinaryCopyRow
    {
        using PooledArrayBufferWriter buffer = new();
        using PgParameterWriter parameterWriter = new(buffer);
        WriteCopyDataMessage(BinaryCopyHeader);
        await FlushStream(cancellationToken).ConfigureAwait(false);
        await foreach (TCopyRow row in rows.ConfigureAwait(false)
                           .WithCancellation(cancellationToken))
        {
            buffer.WriteShort(TCopyRow.ColumnCount);
            row.BindMany(parameterWriter);

            if (buffer.WrittenCount < MaxCopyBufferSize)
            {
                continue;
            }

            WriteCopyDataMessage(buffer.ReadableSpan);
            await FlushStream(cancellationToken).ConfigureAwait(false);
            buffer.Reset();
        }

        if (buffer.WrittenCount > 0)
        {
            WriteCopyDataMessage(buffer.ReadableSpan);
            await FlushStream(cancellationToken).ConfigureAwait(false);
        }
        
        WriteCopyDataMessage(BinaryCopyFooter);
        await FlushStream(cancellationToken).ConfigureAwait(false);
        await SendCopyDoneMessage(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Send a <c>COPY FAIL</c> message to the server with a message as to why the operation failed
    /// </summary>
    /// <param name="failCause">Cause of the copy failure</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    private async Task SendCopyFailAsync(Exception failCause, CancellationToken cancellationToken)
    {
        try
        {
            var message = $"Exception collecting/sending data.\nError:\n{failCause}";
            await SendCopyFailMessage(message, cancellationToken).ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            _logger.LogErrorWhileSendingCopyFail(e);
            BreakConnection();
        }
    }

    /// <summary>
    /// Collect response messages after all copy data is sent to the database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Command complete message sent. This should never be null but the </returns>
    private async Task<QueryResult> CollectCopyInResult(
        CancellationToken cancellationToken)
    {
        // Initialized to avoid returning a nullable type
        var result = new QueryResult(0L, "Default copy in complete message");
        while (true)
        {
            PgBackendMessageType backendMessageType = await ReceiveNextMessageType(cancellationToken)
                .ConfigureAwait(false);
            var size = await ReceiveNextMessageSize(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            switch (backendMessageType)
            {
                case PgBackendMessageType.CommandComplete:
                    result = ReceiveQueryResult(size);
                    break;
                case PgBackendMessageType.ReadyForQuery:
                    HandleReadyForQueryMessage(size);
                    return result;
                default:
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessageType);
                    break;
            }
        }
    }
}
