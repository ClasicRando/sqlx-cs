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
    private bool _isBeforeStart = true;
    private int _statementIndex;

    private PgConnector _connector;
    private readonly ILogger<PgAsyncResultSet> _logger;
    private readonly PgConnector.UserAction _userAction;
    private readonly PgPreparedStatement[] _statements;
    private readonly bool _isSyncAll;
    private PgStatementMetadata? _pgStatementMetadata;

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
        var statementCount = _statements.Length;
        var hasStatements = statementCount > 0;
        if (_pgStatementMetadata is null && hasStatements && _statementIndex >= statementCount)
        {
            return false;
        }

        if (hasStatements)
        {
            _pgStatementMetadata ??=
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
                    _pgStatementMetadata = new PgStatementMetadata(columnMetadata);
                    break;
                case PgBackendMessageType.DataRow:
#pragma warning disable CA2000
                    PgDataRow dataRow =
                        _connector.ReceiveRowDataMessage(size, _pgStatementMetadata!);
#pragma warning restore CA2000
                    Current = Either.Left<IPgDataRow, QueryResult>(dataRow);
                    return true;
                case PgBackendMessageType.CommandComplete:
                    QueryResult queryResult = _connector.ReceiveQueryResult(size);
                    Current = Either.Right<IPgDataRow, QueryResult>(queryResult);
                    if (nextStatementOnCommandComplete)
                    {
                        _pgStatementMetadata = null;
                    }

                    return true;
                case PgBackendMessageType.ReadyForQuery:
                    _connector.HandleReadyForQueryMessage(size);
                    _pgStatementMetadata = null;
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
