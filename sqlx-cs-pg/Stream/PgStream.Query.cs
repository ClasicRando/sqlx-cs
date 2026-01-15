using System.Runtime.CompilerServices;
using Sqlx.Core;
using Sqlx.Core.Cache;
using Sqlx.Core.Config;
using Sqlx.Core.Pool;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Stream;

public partial class PgStream
{
    private const string UnnamedPortal = "";
    private readonly LruCache<string, PgPreparedStatement> _statementCache;
    private int _nextStatementId = 1;

    /// <summary>
    /// True if this query must be executed using the simple protocol. This depends on if there are
    /// any parameters bound to the query, if the connection options allow for using the extended
    /// protocol for simple queries and if the query itself would be a valid extended query.
    /// </summary>
    /// <param name="executableQuery">Query to check</param>
    /// <returns>True if the query must be executed using the simple protocol</returns>
    private bool IsSimpleQuery(IPgExecutableQuery executableQuery)
    {
        if (executableQuery.ParameterCount > 0)
        {
            return false;
        }

        if (!ConnectOptions.UseExtendedProtocolForSimpleQueries)
        {
            return true;
        }

        if (executableQuery.Query.Contains('$'))
        {
            return true;
        }

        return QueryUtils.QueryCount(executableQuery.Query) != 1;
    }

    /// <summary>
    /// <para>
    /// Send a query to the postgres database using the simple query protocol. This sends a single
    /// <c>QUERY</c> message with the raw SQL query and no parameters. The database then responds
    /// with zero or more results that are captured as an async generator of zero or more
    /// <see cref="IDataRow"/>s followed by a <see cref="QueryResult"/> to denote the end of a
    /// result. If the supplied query is a multi-statement query then the result flow will repeat
    /// for every statement that returns results.
    /// </para>
    /// <para>
    /// Note: This method will defer to <see cref="SendExtendedQuery"/> if the query is not a
    /// multi-statement query and <see cref="PgConnectOptions.UseExtendedProtocolForSimpleQueries"/>
    /// is true.
    /// </para>
    /// <a href="https://www.postgresql.org/docs/current/protocol-flow.html#PROTOCOL-FLOW-SIMPLE-QUERY">Postgres docs</a>
    /// </summary>
    /// <param name="sql">Query to execute</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>
    /// Async stream of zero or more data rows followed by a query result to end the result set. For
    /// multi-statement queries, this flow will repeat until all result sets have been sent.
    /// </returns>
    /// <exception cref="ArgumentException">The query is null or whitespace</exception>
    internal async IAsyncEnumerable<Either<IPgDataRow, QueryResult>> SendSimpleQuery(
        string sql,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ThrowIfNotOpen();

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            Status = ConnectionStatus.Executing;
            await SendQueryMessage(sql, cancellationToken).ConfigureAwait(false);
            _pendingReadyForQuery++;

            Status = ConnectionStatus.Fetching;
            var items = CollectResult(null, false, cancellationToken).ConfigureAwait(false);
            await foreach (var item in items)
            {
                yield return item;
            }
        }
        finally
        {
            Status = ConnectionStatus.Idle;
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Send a query to the postgres database using the extended query protocol. This goes through
    /// the process of preparing the query (see <see cref="GetOrPrepareStatement"/>) before
    /// executing with the provided parameters. The database then responds with zero or more results
    /// that are captured as a Flow of zero or more <see cref="IDataRow"/>s followed by a
    /// <see cref="QueryResult"/> to denote the end of a result set.
    /// </summary>
    /// <param name="executableQuery">Query to execute</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>
    /// Async stream of zero or more data rows followed by a query result to end the result set. For
    /// multi-statement queries, this flow will repeat until all result sets have been sent.
    /// </returns>
    /// <exception cref="ArgumentException">The query is null or whitespace</exception>
    /// <exception cref="Sqlx.Postgres.Exceptions.PgException">
    /// If connection status is <see cref="ConnectionStatus.Broken"/> or
    /// <see cref="ConnectionStatus.Closed"/>.
    /// </exception>
    private async IAsyncEnumerable<Either<IPgDataRow, QueryResult>> SendExtendedQuery(
        IPgExecutableQuery executableQuery,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executableQuery.Query);
        ThrowIfNotOpen();

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            Status = ConnectionStatus.Executing;
            PgPreparedStatement statement = await GetOrPrepareStatement(
                    executableQuery.Query,
                    executableQuery.ParameterPgTypes,
                    cancellationToken)
                .ConfigureAwait(false);
            await ExecutePreparedStatement(
                    statement.StatementName,
                    executableQuery.ParameterCount,
                    executableQuery.EncodedParameters,
                    true,
                    cancellationToken)
                .ConfigureAwait(false);
            Status = ConnectionStatus.Fetching;
            var items = CollectResult(statement, false, cancellationToken).ConfigureAwait(false);
            await foreach (var item in items)
            {
                yield return item;
            }
        }
        finally
        {
            Status = ConnectionStatus.Idle;
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryBatch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async IAsyncEnumerable<Either<IPgDataRow, QueryResult>> PipelineQueries(
        IPgQueryBatch queryBatch,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ThrowIfNotOpen();
        PgQueryBatch pgQueryBatch = PgException.CheckIfIs<IPgQueryBatch, PgQueryBatch>(queryBatch);
        var syncAll = queryBatch.WrapBatchInTransaction;

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WaitUntilReady(cancellationToken).ConfigureAwait(false);
            Status = ConnectionStatus.Executing;
            var queries = pgQueryBatch.Queries;
            var queryCount = queries.Count;
            var statements = new PgPreparedStatement[queryCount];

            for (var i = 0; i < queryCount; i++)
            {
                PgExecutableQuery executableQuery = queries[i];
                PgPreparedStatement statement = await GetOrPrepareStatement(
                        executableQuery.Query,
                        executableQuery.ParameterPgTypes,
                        cancellationToken)
                    .ConfigureAwait(false);
                statements[i] = statement;
            }

            for (var i = 0; i < statements.Length; i++)
            {
                PgExecutableQuery executableQuery = queries[i];
                PgPreparedStatement statement = statements[i];
                await ExecutePreparedStatement(
                        statement.StatementName,
                        executableQuery.ParameterCount,
                        executableQuery.EncodedParameters,
                        syncAll || i == queryCount - 1,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            Status = ConnectionStatus.Fetching;
            for (var i = 0; i < statements.Length; i++)
            {
                PgPreparedStatement statement = statements[i];
                var breakOnCommandComplete = !syncAll && i != queryCount - 1;
                var items = CollectResult(statement, breakOnCommandComplete, cancellationToken)
                    .ConfigureAwait(false);
                await foreach (var item in items)
                {
                    yield return item;
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
    /// <para>
    /// Acquire a cached version of this query's prepared statement or create a new prepared
    /// statement and return that instance.
    /// </para>
    /// <para>
    /// New prepared statements will be added to the cache for future usage while also ejecting the
    /// last used prepared statement if the cache is full. This is done transparently since the
    /// cache is an internal implementation detail.
    /// </para>
    /// </summary>
    /// <param name="sql">Query to execute as a prepared statement</param>
    /// <param name="parameterTypes">
    /// Statement parameter types to provide as hints to the database during query parsing
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>The prepared statement to execute the desired query</returns>
    internal async Task<PgPreparedStatement> GetOrPrepareStatement(
        string sql,
        IReadOnlyList<PgTypeInfo> parameterTypes,
        CancellationToken cancellationToken)
    {
        PgPreparedStatement? statement = _statementCache.Get(sql);
        if (statement is not null)
        {
            return statement;
        }

        statement = await ExecuteStatementPrepare(sql, parameterTypes, cancellationToken)
            .ConfigureAwait(false);

        var removedEntry = _statementCache.Put(statement.Sql, statement);
        if (removedEntry is not null)
        {
            await ReleasePreparedStatement(removedEntry.Value.Item2, cancellationToken)
                .ConfigureAwait(false);
        }

        return statement;
    }

    /// <summary>
    /// Bind parameters to a new portal for the named statement and execute the same portal to
    /// fetch all rows.
    /// </summary>
    /// <param name="statementName">Name of the statement to execute</param>
    /// <param name="parameterCount">Query to execute</param>
    /// <param name="encodedParameters">Query to execute</param>
    /// <param name="sendSync">
    /// True if the messages should be finalized with a SYNC command. Otherwise, the messages are
    /// just flushed to the server
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private ValueTask ExecutePreparedStatement(
        string statementName,
        short parameterCount,
        in ReadOnlySpan<byte> encodedParameters,
        bool sendSync,
        CancellationToken cancellationToken)
    {
        WriteBindMessage(
            UnnamedPortal,
            statementName,
            parameterCount,
            encodedParameters);
        WriteExecuteMessage(UnnamedPortal, 0);
        WriteCloseMessage(MessageTarget.Portal, UnnamedPortal);
        return sendSync ? WriteSync(cancellationToken) : FlushStream(cancellationToken);
    }

    /// <summary>
    /// Collects all results from the prepared statement or simple query execution. This is an async
    /// statement machine that processes messages until finding a specific message then it exists
    /// the state machine loop. Along the way, <see cref="IDataRow"/> and <see cref="QueryResult"/>
    /// instances are emitted for downstream processing.
    /// </summary>
    /// <param name="preparedStatement">
    /// Optional prepared statement. If specified the query flow is the extended query protocol and
    /// a row description message is not expected before query response. If null, the query flow is
    /// the simple query protocol and a row description message is expected before the query
    /// response.
    /// </param>
    /// <param name="breakOnCommandComplete">
    /// True if the enumerable should break when <c>COMMAND COMPLETE</c> is sent by the server. This
    /// should only be true for pipelined queries where <c>SYNC</c> is not sent for every query in
    /// the batch. In other words, this parameter is telling the method that <c>READY FOR QUERY</c>
    /// is not the only exit condition and command complete should also be considered a condition to
    /// exit result collection.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>An async stream of result rows and query results</returns>
    private async IAsyncEnumerable<Either<IPgDataRow, QueryResult>> CollectResult(
        PgPreparedStatement? preparedStatement,
        bool breakOnCommandComplete,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var statementMetadata = new PgStatementMetadata(preparedStatement?.ColumnMetadata ?? []);
        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = ApplyStandardMessageProcessing(
                    backendMessage);
            cancellationToken.ThrowIfCancellationRequested();
            switch (postProcessMessage)
            {
                case RowDescriptionMessage rowDescription:
                    statementMetadata = new PgStatementMetadata(rowDescription.ColumnMetadata);
                    break;
                case DataRowMessage dataRowMessage:
                    var dataRow = new PgDataRow(dataRowMessage.RowData, statementMetadata);
                    yield return new Either<IPgDataRow, QueryResult>.Left(dataRow);
                    break;
                case CommandCompleteMessage commandCompleteMessage:
                    var queryResult = new QueryResult(
                        commandCompleteMessage.RowCount,
                        commandCompleteMessage.Message);
                    yield return new Either<IPgDataRow, QueryResult>.Right(queryResult);
                    if (breakOnCommandComplete)
                    {
                        yield break;
                    }

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

    /// <summary>
    /// Sends a <see cref="PgFrontendMessageType.Sync"/> message and increments the
    /// <see cref="_pendingReadyForQuery"/> counter to indicate that a ready for query message
    /// should be sent by the server before proceeding with another query flow.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private async ValueTask WriteSync(CancellationToken cancellationToken)
    {
        await SendSyncMessage(cancellationToken).ConfigureAwait(false);
        _pendingReadyForQuery++;
    }

    /// <summary>
    /// Send a <see cref="PgFrontendMessageType.Close"/> message for the prepared statement. This
    /// will close the server side prepared statement.
    /// </summary>
    /// <param name="preparedStatement">Prepared statement to close</param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private async Task ReleasePreparedStatement(
        PgPreparedStatement preparedStatement,
        CancellationToken cancellationToken)
    {
        WriteCloseMessage(
            MessageTarget.PreparedStatement,
            preparedStatement.StatementName);
        await WriteSync(cancellationToken).ConfigureAwait(false);
        await WaitUntilReady(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// <para>
    /// Create a <see cref="PgPreparedStatement"/> using the supplied query and parameter types.
    /// </para>
    /// <para>
    /// Internally this parses the query and requests the server describe the prepared statement
    /// result columns for future result deserialization.
    /// </para>
    /// </summary>
    /// <param name="sql">Query to create a prepared statement for</param>
    /// <param name="parameterTypes">
    /// Readonly list of types associated with the prepared statement parameters
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <returns>
    /// A new prepared statement instance associated with a server prepared statement
    /// </returns>
    private async Task<PgPreparedStatement> ExecuteStatementPrepare(
        string sql,
        IReadOnlyList<PgTypeInfo> parameterTypes,
        CancellationToken cancellationToken)
    {
        var statement = new PgPreparedStatement(sql, _nextStatementId++);
        WriteParseMessage(statement.StatementName, sql, parameterTypes);
        WriteDescribeMessage(MessageTarget.PreparedStatement, statement.StatementName);
        await WriteSync(cancellationToken).ConfigureAwait(false);

        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = ApplyStandardMessageProcessing(
                    backendMessage);
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
