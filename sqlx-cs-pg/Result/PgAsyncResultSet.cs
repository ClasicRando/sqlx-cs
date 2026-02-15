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
    private bool _disposed;
    private bool _isComplete;
    private bool _isBeforeStart = true;

    private PgConnector _connector;
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
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _isBeforeStart
                ? throw new InvalidOperationException(
                    "Attempted to view current item before starting result collection")
                : field;
        }
        private set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_isBeforeStart && field.IsLeft)
            {
                field.Left.Dispose();
            }
            field = value;
        }
    }

    public async ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _isBeforeStart = false;
        if (_isComplete)
        {
            return false;
        }

        while (true)
        {
            PgBackendMessageType backendMessageType = await _connector
                .ReceiveNextMessageType(cancellationToken)
                .ConfigureAwait(false);
            var size = await _connector.ReceiveNextMessageSize(cancellationToken)
                .ConfigureAwait(false);

            if (_connector.ApplyStandardMessageProcessing(backendMessageType, size)) continue;
            
            cancellationToken.ThrowIfCancellationRequested();
            switch (backendMessageType)
            {
                case PgBackendMessageType.RowDescription:
                    var columnMetadata = _connector.ReceiveRowDescriptionMessage(size);
                    _pgStatementMetadata = new PgStatementMetadata(columnMetadata);
                    break;
                case PgBackendMessageType.DataRow:
#pragma warning disable CA2000
                    PgDataRow dataRow = _connector.ReceiveRowDataMessage(size, _pgStatementMetadata);
#pragma warning restore CA2000
                    Current = Either.Left<IPgDataRow, QueryResult>(dataRow);
                    return true;
                case PgBackendMessageType.CommandComplete:
                    var commandCompleteMessage = _connector.ReceiveMessage<CommandCompleteMessage>(size);
                    var queryResult = new QueryResult(
                        commandCompleteMessage.RowCount,
                        commandCompleteMessage.Message);
                    Current = Either.Right<IPgDataRow, QueryResult>(queryResult);
                    return true;
                case PgBackendMessageType.ReadyForQuery:
                    _connector.HandleReadyForQueryMessage(size);
                    _isComplete = true;
                    return false;
                case PgBackendMessageType.BindComplete:
                case PgBackendMessageType.ParseComplete:
                case PgBackendMessageType.ParameterDescription:
                case PgBackendMessageType.NoData:
                case PgBackendMessageType.CloseComplete:
                    _connector.AdvanceReadBuffer(size);
                    break;
                default:
                    _connector.AdvanceReadBuffer(size);
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessageType);
                    break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _userAction.Dispose();
        if (!_isBeforeStart && Current.IsLeft)
        {
            Current.Left.Dispose();
        }

        _disposed = true;
        _connector = null!;
    }
}
