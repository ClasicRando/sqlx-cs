using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Config;
using Sqlx.Core.Result;
using Sqlx.Postgres.Connector;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Result;

internal sealed class PgAsyncResultSet : AbstractPgDataRow, IAsyncResultSet<IPgDataRow>
{
    private bool _disposed;
    private bool _isBeforeStart = true;
    private int _statementIndex;

    private PgConnector _connector;
    private readonly ILogger<PgAsyncResultSet> _logger;
    private readonly PgConnector.UserAction _userAction;
    private readonly PgPreparedStatement[] _statements;
    private readonly bool _isSyncAll;

    public PgAsyncResultSet(
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
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _isBeforeStart
                ? throw new InvalidOperationException(
                    "Attempted to view current item before starting result collection")
                : field;
        }
        private set;
    }

    public async ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _isBeforeStart = false;
        RowData = default;
        var statementCount = _statements.Length;
        var hasStatements = statementCount > 0;
        if (StatementMetadata is null && hasStatements && _statementIndex >= statementCount)
        {
            return false;
        }

        if (hasStatements)
        {
            StatementMetadata ??=
                new PgStatementMetadata(_statements[_statementIndex++].ColumnMetadata);
        }

        var nextStatementOnCommandComplete =
            !_isSyncAll && hasStatements && _statementIndex != statementCount;

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
                    StatementMetadata = new PgStatementMetadata(columnMetadata);
                    break;
                case PgBackendMessageType.DataRow:
                    RowData = _connector.ReceiveRowDataMessage(size);
                    Current = Either.Left<IPgDataRow, QueryResult>(this);
                    return true;
                case PgBackendMessageType.CommandComplete:
                    QueryResult queryResult = _connector.ReceiveQueryResult(size);
                    Current = Either.Right<IPgDataRow, QueryResult>(queryResult);
                    if (nextStatementOnCommandComplete)
                    {
                        StatementMetadata = null;
                    }

                    return true;
                case PgBackendMessageType.ReadyForQuery:
                    _connector.HandleReadyForQueryMessage(size);
                    StatementMetadata = null;
                    return _connector.PendingReadyForQuery > 0;
                case PgBackendMessageType.BindComplete:
                case PgBackendMessageType.ParseComplete:
                case PgBackendMessageType.ParameterDescription:
                case PgBackendMessageType.NoData:
                case PgBackendMessageType.CloseComplete:
                case PgBackendMessageType.EmptyQueryResponse:
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

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;

        _userAction.Dispose();

        _disposed = true;
        _connector = null!;
    }
}
