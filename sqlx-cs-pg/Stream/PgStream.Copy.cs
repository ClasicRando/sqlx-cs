using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Sqlx.Core.Config;
using Sqlx.Core.Pool;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Stream;

public sealed partial class PgStream
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

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            Status = ConnectionStatus.Executing;
            await SendQueryMessage(copyOutStatement.ToCopyQuery(), cancellationToken)
                .ConfigureAwait(false);
            await WaitForOrThrowError<CopyOutResponseMessage>(cancellationToken);
            _pendingReadyForQuery++;

            Status = ConnectionStatus.Fetching;
            while (true)
            {
                IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                    .ConfigureAwait(false);
                IPgBackendMessage? postProcessMessage =
                    ApplyStandardMessageProcessing(backendMessage);
                cancellationToken.ThrowIfCancellationRequested();
                switch (postProcessMessage)
                {
                    case CopyDataMessage copyDataMessage:
                        yield return copyDataMessage.Data;
                        break;
                    case CopyDoneMessage:
                    case CommandCompleteMessage:
                        break;
                    case ReadyForQueryMessage readyForQueryMessage:
                        HandleReadyForQuery(readyForQueryMessage);
                        yield break;
                    default:
                        _logger.LogIgnoreUnexpectedMessage(
                            SqlxConfig.DetailedLoggingLevel,
                            backendMessage);
                        break;
                }
            }
        }
        finally
        {
            Status = ConnectionStatus.Idle;
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Executes a <c>COPY FROM</c> statement against this connection. Initiates the copy operation
    /// followed by forwarding all data received from the <see cref="PipeReader"/> to the database
    /// as <see cref="CopyDataMessage"/>s. 
    /// </summary>
    /// <param name="copyInStatement">Copy from statement</param>
    /// <param name="data">Data pipe containing all user supplied data</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Query result data</returns>
    internal async Task<QueryResult> CopyIn(
        ICopyFrom copyInStatement,
        PipeReader data,
        CancellationToken cancellationToken)
    {
        ThrowIfNotOpen();

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            Status = ConnectionStatus.Executing;
            await SendQueryMessage(copyInStatement.ToCopyQuery(), cancellationToken)
                .ConfigureAwait(false);
            await WaitForOrThrowError<CopyInResponseMessage>(cancellationToken);
            _pendingReadyForQuery++;

            try
            {
                await SendCopyDataAsync(data, cancellationToken);
            }
            catch (Exception e)
            {
                await SendCopyFailAsync(cancellationToken: cancellationToken, failCause: e);
                throw;
            }

            CommandCompleteMessage completedMessage = await CollectCopyInResult(cancellationToken)
                .ConfigureAwait(false);
            return new QueryResult(completedMessage.RowCount, completedMessage.Message);
        }
        finally
        {
            await data.CompleteAsync().ConfigureAwait(false);
            Status = ConnectionStatus.Idle;
            _semaphore.Release();
        }
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
    private async Task SendCopyDataAsync(PipeReader data, CancellationToken cancellationToken)
    {
        var bytesWritten = 0;
        while (true)
        {
            ReadResult readResult = await data.ReadAsync(cancellationToken)
                .ConfigureAwait(false);
            if (readResult.IsCanceled)
            {
                throw new PgException(
                    $"Data stream supplied to {nameof(PgConnection.CopyInAsync)} was cancelled before completion");
            }

            foreach (var chunk in readResult.Buffer)
            {
                if (bytesWritten > MaxCopyDataSendSize)
                {
                    await FlushStream(cancellationToken).ConfigureAwait(false);
                }

                WriteCopyDataMessage(chunk.Span);
                bytesWritten += chunk.Length;
            }

            data.AdvanceTo(readResult.Buffer.End, readResult.Buffer.End);

            if (!readResult.IsCompleted)
            {
                continue;
            }

            await FlushStream(cancellationToken).ConfigureAwait(false);
            await SendCopyDoneMessage(cancellationToken).ConfigureAwait(false);
            break;
        }
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
        catch
        {
            BreakConnection();
        }
    }

    /// <summary>
    /// Collect response messages after all copy data is sent to the database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Command complete message sent. This should never be null but the </returns>
    private async Task<CommandCompleteMessage> CollectCopyInResult(
        CancellationToken cancellationToken)
    {
        // Initialized to avoid returning a nullable type
        var result = new CommandCompleteMessage(0, "Default copy in complete message");
        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = ApplyStandardMessageProcessing(
                    backendMessage);
            cancellationToken.ThrowIfCancellationRequested();
            switch (postProcessMessage)
            {
                case CommandCompleteMessage commandCompleteMessage:
                    result = commandCompleteMessage;
                    break;
                case ReadyForQueryMessage readyForQueryMessage:
                    HandleReadyForQuery(readyForQueryMessage);
                    return result;
                default:
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessage);
                    break;
            }
        }
    }
}
