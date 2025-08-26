using System.Buffers;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;
using Sqlx.Postgres.Notify;
using Sqlx.Core.Config;
using Sqlx.Postgres.Logging;

namespace Sqlx.Postgres.Stream;

internal sealed partial class PgStream : IAsyncDisposable
{
    private readonly IAsyncStream _asyncStream;
    internal readonly PgConnectOptions ConnectOptions;
    private readonly ILogger _logger;
    private readonly WriteBuffer _buffer = new();
    private readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    private readonly Channel<PgNotification> _notifications = Channel.CreateUnbounded<PgNotification>();
    private BackendDataKeyMessage? _backendDataKey;
    
    internal PgStream(IAsyncStream asyncStream, PgConnectOptions connectOptions)
    {
        _asyncStream = asyncStream;
        ConnectOptions = connectOptions;
        _logger = connectOptions.LoggerFactory.CreateLogger<PgStream>();
    }

    internal bool IsConnected => _asyncStream.IsConnected;
    
    internal async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _asyncStream.OpenAsync(ConnectOptions.Host, ConnectOptions.Port, cancellationToken)
            .ConfigureAwait(false);
        await SendMessage(new StartupMessage(ConnectOptions), cancellationToken)
            .ConfigureAwait(false);
        await HandleAuthFlow(cancellationToken).ConfigureAwait(false);
    }
    
    private async Task HandleAuthFlow(CancellationToken cancellationToken)
    {
        var authentication = await ReceiveNextMessageAs<AuthenticationMessage>(cancellationToken)
            .ConfigureAwait(false);
        switch (authentication.AuthMessage)
        {
            case OkAuthMessage:
                // logged in!
                break;
            case ClearTextPasswordAuthMessage:
                await SimplePasswordAuthFlow(
                    ConnectOptions.Username,
                    ConnectOptions.Password ?? throw new PgException("Cannot connect to database without Password property"),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                break;
            case MD5PasswordAuthMessage md5PasswordAuthMessage:
                await SimplePasswordAuthFlow(
                    ConnectOptions.Username,
                    ConnectOptions.Password ?? throw new PgException("Cannot connect to database without Password property"),
                    salt: md5PasswordAuthMessage.Salt,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                break;
            case SaslAuthMessage saslAuthMessage:
                await SaslAuthFlow(
                    ConnectOptions.Password ?? throw new PgException("Cannot connect to database without Password property"),
                    saslAuthMessage, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new PgException($"Auth request type cannot be handled. {authentication}");
        }
    }

    private async Task<PgNotification> WaitForNotificationOrError(CancellationToken cancellationToken)
    {
        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = await ApplyStandardMessageProcessing(
                backendMessage,
                handleNotification: false,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (postProcessMessage is NotificationResponseMessage result)
            {
                return new PgNotification(result.ProcessId, result.ChannelName, result.Payload);
            }
            
            _logger.LogIgnoreUnexpectedMessage(
                SqlxConfig.DetailedLoggingLevel,
                backendMessage);
        }
    }

    internal async Task<T> WaitForOrThrowError<T>(CancellationToken cancellationToken) where T : IPgBackendMessage
    {
        var result = await WaitForOrError<T>(cancellationToken)
            .ConfigureAwait(false);
        if (result.Left is not null)
        {
            return result.Left;
        }
        throw new PgException(result.Right!);
    }

    internal async Task<Either<T, ErrorResponseMessage>> WaitForOrError<T>(CancellationToken cancellationToken) where T : IPgBackendMessage
    {
        while (true)
        {
            IPgBackendMessage backendMessage = await ReceiveNextMessage(cancellationToken)
                .ConfigureAwait(false);
            IPgBackendMessage? postProcessMessage = await ApplyStandardMessageProcessing(
                backendMessage,
                throwOnError: false,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            switch (postProcessMessage)
            {
                case ErrorResponseMessage errorResponse:
                    return Either<T, ErrorResponseMessage>.OfRight(errorResponse);
                case T result:
                    return Either<T, ErrorResponseMessage>.OfLeft(result);
                default:
                    _logger.LogIgnoreUnexpectedMessage(
                        SqlxConfig.DetailedLoggingLevel,
                        backendMessage);
                    break;
            }
        }
    }

    internal async Task<IPgBackendMessage?> ApplyStandardMessageProcessing(
        IPgBackendMessage message,
        bool throwOnError = true,
        bool handleNotification = true,
        CancellationToken cancellationToken = default)
    {
        switch (message)
        {
            case NoticeResponseMessage noticeResponse:
                OnNotice(noticeResponse);
                return null;
            case NotificationResponseMessage notificationResponse:
                if (!handleNotification) return notificationResponse;
                await OnNotification(notificationResponse, cancellationToken).ConfigureAwait(false);
                return null;
            case BackendDataKeyMessage backendDataKey:
                OnBackendDataKey(backendDataKey);
                return null;
            case ParameterStatusMessage parameterStatus:
                OnParameterStatus(parameterStatus);
                return null;
            case NegotiateProtocolVersionMessage negotiateProtocolVersion:
                OnNegotiateProtocolVersion(negotiateProtocolVersion);
                return null;
            case ErrorResponseMessage errorResponse:
                if (throwOnError) throw new PgException(errorResponse);
                return message;
            default:
                return message;
        }
    }

    private void OnNotice(NoticeResponseMessage noticeResponse)
    {
        _logger.LogNotice(SqlxConfig.DetailedLoggingLevel, noticeResponse);
    }

    private ValueTask OnNotification(NotificationResponseMessage message, CancellationToken cancellationToken)
    {
        PgNotification notification = new(message.ProcessId, message.ChannelName, message.Payload);
        return _notifications.Writer.WriteAsync(notification, cancellationToken);
    }

    private void OnBackendDataKey(BackendDataKeyMessage message)
    {
        _logger.LogBackendDataKey(SqlxConfig.DetailedLoggingLevel, message.ProcessId);
        _backendDataKey = message;
    }

    private void OnParameterStatus(ParameterStatusMessage message)
    {
        _logger.LogParameterStatus(SqlxConfig.DetailedLoggingLevel, message.Name);
    }

    private void OnNegotiateProtocolVersion(NegotiateProtocolVersionMessage message)
    {
        _logger.LogNegotiateProtocolVersion(
            SqlxConfig.DetailedLoggingLevel,
            message.MinProtocolVersion,
            message.ProtocolOptionsNotRecognized);
    }

    internal async Task<IPgBackendMessage> ReceiveNextMessage(CancellationToken cancellationToken)
    {
        var format = (PgBackendMessageType)await _asyncStream.ReadByteAsync(cancellationToken).ConfigureAwait(false);
        var size = await _asyncStream.ReadIntAsync(cancellationToken).ConfigureAwait(false);
        var contents = _arrayPool.Rent(size);
        try
        {
            await _asyncStream.ReadBuffer(contents.AsMemory(), cancellationToken).ConfigureAwait(false);
            return ParseMessage(format, contents);
        }
        finally
        {
            _arrayPool.Return(contents);
        }
    }

    private async Task<T> ReceiveNextMessageAs<T>(CancellationToken cancellationToken)
        where T : IPgBackendMessage
    {
        IPgBackendMessage message = await ReceiveNextMessage(cancellationToken).ConfigureAwait(false);
        return message switch
        {
            T result => result,
            ErrorResponseMessage errorResponse => throw new PgException(errorResponse),
            _ => throw new PgException(
                $"Expected {typeof(T)} message but found {message.GetType()}"),
        };
    }

    internal void WriteMessage<T>(T message) where T : IPgFrontendMessage
    {
        message.Encode(_buffer);
    }

    internal ValueTask SendMessage<T>(T message, CancellationToken cancellationToken)
        where T : IPgFrontendMessage
    {
        message.Encode(_buffer);
        return Flush(cancellationToken);
    }

    private ValueTask Flush(CancellationToken cancellationToken)
    {
        return _asyncStream.WriteAsync(_buffer.Memory, cancellationToken);
    }

    private static IPgBackendMessage ParseMessage(
        PgBackendMessageType messageType,
        byte[] contents)
    {
        ReadBuffer buffer = new(contents);
        return messageType switch
        {
            PgBackendMessageType.Authentication => AuthenticationMessage.Decode(buffer),
            PgBackendMessageType.BackendDataKey => BackendDataKeyMessage.Decode(buffer),
            PgBackendMessageType.BindComplete => BindCompleteMessage.Instance,
            PgBackendMessageType.CloseComplete => CloseCompleteMessage.Instance,
            PgBackendMessageType.CommandComplete => CommandCompleteMessage.Decode(buffer),
            PgBackendMessageType.CopyData => CopyDataMessage.Decode(buffer),
            PgBackendMessageType.CopyDone => CopyDoneMessage.Instance,
            PgBackendMessageType.CopyInResponse => CopyInResponseMessage.Decode(buffer),
            PgBackendMessageType.CopyOutResponse => CopyOutResponseMessage.Decode(buffer),
            PgBackendMessageType.CopyBothResponse => throw new PgException(
                "CopyBoth is not supported by this driver"),
            PgBackendMessageType.DataRow => DataRowMessage.Decode(buffer),
            PgBackendMessageType.EmptyQueryResponse => throw new PgException(
                "Empty query response packet should never be received"),
            PgBackendMessageType.ErrorResponse => ErrorResponseMessage.Decode(buffer),
            PgBackendMessageType.FunctionCallResponse => FunctionCallResponseMessage.Decode(buffer),
            PgBackendMessageType.NegotiateProtocolVersion =>
                NegotiateProtocolVersionMessage.Decode(buffer),
            PgBackendMessageType.NoData => NoDataMessage.Instance,
            PgBackendMessageType.NoticeResponse => NoticeResponseMessage.Decode(buffer),
            PgBackendMessageType.NotificationResponse => NotificationResponseMessage.Decode(buffer),
            PgBackendMessageType.ParameterDescription => ParameterDescriptionMessage.Decode(buffer),
            PgBackendMessageType.ParameterStatus => ParameterStatusMessage.Decode(buffer),
            PgBackendMessageType.ParseComplete => ParseCompleteMessage.Instance,
            PgBackendMessageType.PortalSuspend => PortalSuspendedMessage.Instance,
            PgBackendMessageType.ReadyForQuery => ReadyForQueryMessage.Decode(buffer),
            PgBackendMessageType.RowDescription => RowDescriptionMessage.Decode(buffer),
            _ => throw new PgException($"Expected backend message type but found '{messageType}'"),
        };
    }

    internal async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (!_asyncStream.IsConnected)
        {
            return;
        }

        await _asyncStream.CloseAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        _buffer.Dispose();
        await CloseAsync();
    }
}
