using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Config;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connector;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Result;

internal sealed class PgAsyncResultSet : IAsyncResultSet<IPgDataRow>
{
    private bool _isComplete;
    private bool _isBeforeStart = true;

    private readonly PgConnector _connector;
    private readonly ILogger<PgAsyncResultSet> _logger;
    private readonly PgConnector.UserAction _userAction;
    private PgStatementMetadata _pgStatementMetadata;

    public PgAsyncResultSet(
        PgConnector connector,
        PgConnector.UserAction userAction,
        PgPreparedStatement? preparedStatement)
    {
        _connector = connector;
        _logger = connector.ConnectOptions.LoggerFactory.CreateLogger<PgAsyncResultSet>();
        _userAction = userAction;
        _pgStatementMetadata = new PgStatementMetadata(preparedStatement?.ColumnMetadata ?? []);
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
        if (_isComplete)
        {
            return false;
        }

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
                case RowDescriptionMessage rowDescription:
                    _pgStatementMetadata = new PgStatementMetadata(rowDescription.ColumnMetadata);
                    break;
                case DataRowMessage dataRowMessage:
                    var dataRow = new PgDataRow(dataRowMessage.RowData, _pgStatementMetadata);
                    Current = Either.Left<IPgDataRow, QueryResult>(dataRow);
                    return true;
                case CommandCompleteMessage commandCompleteMessage:
                    var queryResult = new QueryResult(
                        commandCompleteMessage.RowCount,
                        commandCompleteMessage.Message);
                    Current = Either.Right<IPgDataRow, QueryResult>(queryResult);
                    return true;
                case ReadyForQueryMessage readyForQueryMessage:
                    _connector.HandleReadyForQuery(readyForQueryMessage);
                    _isComplete = true;
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
