using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Config;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connector;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Result;

internal class PgBatchAsyncResultSet : IAsyncResultSet<IPgDataRow>
{
    private bool _isBeforeStart = true;
    private int _statementIndex;

    private readonly PgConnector _connector;
    private readonly ILogger<PgAsyncResultSet> _logger;
    private readonly PgConnector.UserAction _userAction;
    private readonly PgPreparedStatement[] _statements;
    private readonly bool _isSyncAll;
    private PgStatementMetadata? _pgStatementMetadata;

    public PgBatchAsyncResultSet(
        PgConnector connector,
        PgConnector.UserAction userAction,
        PgPreparedStatement[] statements,
        bool isSyncAll)
    {
        _connector = connector;
        _logger = connector.ConnectOptions.LoggerFactory.CreateLogger<PgAsyncResultSet>();
        _userAction = userAction;
        _statements = statements;
        _isSyncAll = isSyncAll;
    }

    public Either<IPgDataRow, QueryResult> Current
    {
        get => _isBeforeStart
            ? throw new InvalidOperationException(
                "Attempted to view current item before starting result collection")
            : field;
        private set;
    }

    public async ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        _isBeforeStart = false;
        var statementCount = _statements.Length;
        if (_pgStatementMetadata is null && _statementIndex >= statementCount)
        {
            return false;
        }

        _pgStatementMetadata ??=
            new PgStatementMetadata(_statements[_statementIndex++].ColumnMetadata);
        var nextStatementOnCommandComplete = !_isSyncAll && _statementIndex != statementCount;

        while (true)
        {
            IPgBackendMessage backendMessage = await _connector
                .ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = _connector.ApplyStandardMessageProcessing(
                backendMessage);
            cancellationToken.ThrowIfCancellationRequested();
            switch (postProcessMessage)
            {
                case DataRowMessage dataRowMessage:
                    var dataRow = new PgDataRow(dataRowMessage.RowData, _pgStatementMetadata);
                    Current = Either.Left<IPgDataRow, QueryResult>(dataRow);
                    return true;
                case CommandCompleteMessage commandCompleteMessage:
                    var queryResult = new QueryResult(
                        commandCompleteMessage.RowCount,
                        commandCompleteMessage.Message);
                    Current = Either.Right<IPgDataRow, QueryResult>(queryResult);
                    if (nextStatementOnCommandComplete)
                    {
                        _pgStatementMetadata = null;
                    }

                    return true;
                case ReadyForQueryMessage readyForQueryMessage:
                    _connector.HandleReadyForQuery(readyForQueryMessage);
                    _pgStatementMetadata = null;
                    return false;
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

    public void Dispose()
    {
        _userAction.Dispose();
    }
}
