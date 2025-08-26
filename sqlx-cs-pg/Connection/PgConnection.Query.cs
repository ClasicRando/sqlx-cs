using System.Runtime.CompilerServices;
using Sqlx.Core;
using Sqlx.Core.Cache;
using Sqlx.Core.Config;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Connection;

public partial class PgConnection
{
    private readonly LruCache<string, PgPreparedStatement> _statementCache;
    private int _nextStatementId = 1;
    
    private async IAsyncEnumerable<Either<IDataRow, QueryResult>> SendSimpleQuery(
        string sql,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ThrowIfNotConnected();

        if (_pgStream.ConnectOptions.UseExtendedProtocolForSimpleQueries && !sql.Contains('$'))
        {
            if (QueryUtils.QueryCount(sql) == 1)
            {
                using var buffer = new PgParameterBuffer();
                var items = SendExtendedQuery(sql, buffer, cancellationToken)
                    .ConfigureAwait(false);
                await foreach (var item in items)
                {
                    yield return item;
                }
            }
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            await _pgStream.SendMessage(new QueryMessage(sql), cancellationToken)
                .ConfigureAwait(false);
            _pendingReadyForQuery++;

            var items = CollectResult(null, cancellationToken).ConfigureAwait(false);
            await foreach (var item in items)
            {
                yield return item;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async IAsyncEnumerable<Either<IDataRow, QueryResult>> SendExtendedQuery(
        string sql,
        PgParameterBuffer parameterBuffer,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ThrowIfNotConnected();
        
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            PgPreparedStatement statement = await GetOrPrepareStatement(
                sql,
                parameterBuffer,
                cancellationToken)
                .ConfigureAwait(false);
            await ExecutePreparedStatement(statement, parameterBuffer, true, cancellationToken)
                .ConfigureAwait(false);
            
            var items = CollectResult(null, cancellationToken).ConfigureAwait(false);
            await foreach (var item in items)
            {
                yield return item;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<PgPreparedStatement> GetOrPrepareStatement(
        string sql,
        PgParameterBuffer parameterBuffer,
        CancellationToken cancellationToken)
    {
        PgPreparedStatement? statement = _statementCache.Get(sql);
        if (statement is not null)
        {
            return statement;
        }

        statement = await ExecuteStatementPrepare(sql, parameterBuffer.PgTypes, cancellationToken)
            .ConfigureAwait(false);
        
        var removedEntry = _statementCache.Put(sql, statement);
        if (removedEntry is not null)
        {
            await ReleasePreparedStatement(removedEntry.Value.Item2, cancellationToken)
                .ConfigureAwait(false);
        }

        return statement;
    }

    private Task ExecutePreparedStatement(
        PgPreparedStatement preparedStatement,
        PgParameterBuffer parameterBuffer,
        bool sendSync,
        CancellationToken cancellationToken)
    {
        var bindMessage = new BindMessage(
            null,
            preparedStatement.StatementName,
            parameterBuffer.ParameterCount,
            parameterBuffer.Memory);
        _pgStream.WriteMessage(bindMessage);
        _pgStream.WriteMessage(new ExecuteMessage(null, 0));
        _pgStream.WriteMessage(new CloseMessage(MessageTarget.Portal, null));
        return sendSync ? WriteSync(cancellationToken) : Task.CompletedTask;
    }

    private async IAsyncEnumerable<Either<IDataRow, QueryResult>> CollectResult(
        PgPreparedStatement? preparedStatement,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var columnMetadata = preparedStatement?.ColumnMetadata ?? [];
        while (true)
        {
            IPgBackendMessage backendMessage = await _pgStream.ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = await _pgStream.ApplyStandardMessageProcessing(
                backendMessage,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            switch (postProcessMessage)
            {
                case RowDescriptionMessage rowDescription:
                    if (preparedStatement is not null)
                    {
                        preparedStatement.ColumnMetadata = rowDescription.ColumnMetadata;
                    }
                    columnMetadata = rowDescription.ColumnMetadata;
                    break;
                case DataRowMessage dataRowMessage:
                    var dataRow = new PgDataRow(dataRowMessage.RowData, columnMetadata);
                    yield return Either<IDataRow, QueryResult>.OfLeft(dataRow);
                    break;
                case CommandCompleteMessage commandCompleteMessage:
                    var queryResult = new QueryResult(
                        commandCompleteMessage.RowCount,
                        commandCompleteMessage.Message);
                    yield return Either<IDataRow, QueryResult>.OfRight(queryResult);
                    break;
                case ReadyForQueryMessage readyForQueryMessage:
                    HandleReadyForQuery(readyForQueryMessage);
                    yield break;
                case BindCompleteMessage:
                case ParseCompleteMessage:
                case ParameterDescriptionMessage:
                case NoDataMessage:
                case CloseCompleteMessage:
                    break;
                default:
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessage);
                    break;
            }
        }
    }

    private async Task WriteSync(CancellationToken cancellationToken)
    {
        await _pgStream.SendMessage(SyncMessage.Instance, cancellationToken).ConfigureAwait(false);
        _pendingReadyForQuery++;
    }

    private async Task ReleasePreparedStatement(PgPreparedStatement preparedStatement, CancellationToken cancellationToken)
    {
        var closeMessage = new CloseMessage(
            MessageTarget.PreparedStatement,
            preparedStatement.StatementName);
        _pgStream.WriteMessage(closeMessage);
        await WriteSync(cancellationToken).ConfigureAwait(false);
        await WaitUntilReady(cancellationToken).ConfigureAwait(false);
    }

    private async Task<PgPreparedStatement> ExecuteStatementPrepare(string sql, IReadOnlyList<PgType> parameterTypes, CancellationToken cancellationToken)
    {
        var statement = new PgPreparedStatement(sql, _nextStatementId++);
        _pgStream.WriteMessage(new ParseMessage(statement.StatementName, sql, parameterTypes));
        _pgStream.WriteMessage(new DescribeMessage(MessageTarget.PreparedStatement, statement.StatementName));
        await WriteSync(cancellationToken).ConfigureAwait(false);
        
        while (true)
        {
            IPgBackendMessage backendMessage = await _pgStream.ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = await _pgStream.ApplyStandardMessageProcessing(
                backendMessage,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            switch (postProcessMessage)
            {
                case ParseCompleteMessage:
                case ParameterDescriptionMessage:
                    break;
                case RowDescriptionMessage rowDescriptionMessage:
                    statement.ColumnMetadata = rowDescriptionMessage.ColumnMetadata
                        .Select(c => c.WithBinaryFormat())
                        .ToArray();
                    break;
                case NoDataMessage:
                    statement.ColumnMetadata = [];
                    break;
                case ReadyForQueryMessage readyForQueryMessage:
                    HandleReadyForQuery(readyForQueryMessage);
                    return statement;
                default:
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessage);
                    break;
            }
        }
    }
}
