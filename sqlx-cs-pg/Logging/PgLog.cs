using Microsoft.Extensions.Logging;
using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Logging;

internal static partial class PgLog
{
    [LoggerMessage(Message = "Notice -> {@NoticeResponse}")]
    internal static partial void LogNotice(
        this ILogger logger,
        LogLevel logLevel,
        NoticeResponseMessage noticeResponse);
    
    [LoggerMessage(Message = "Parameter updated. Name: {Name}")]
    internal static partial void LogParameterStatus(
        this ILogger logger,
        LogLevel logLevel,
        string name);

    [LoggerMessage(Message = "Got backend key data. Process ID: {ProcessId}")]
    internal static partial void LogBackendDataKey(
        this ILogger logger,
        LogLevel logLevel,
        int processId);
    
    [LoggerMessage(Message = "Server does not support protocol version 3.0. Min Protocol Support: {MinProtocolVersion}, Unknown Options: {ProtocolOptionsNotRecognized}")]
    internal static partial void LogNegotiateProtocolVersion(
        this ILogger logger,
        LogLevel logLevel,
        int minProtocolVersion,
        string[] protocolOptionsNotRecognized);
    
    [LoggerMessage(Message = "Ignoring {message} since it's not an error or the desired type")]
    internal static partial void LogIgnoreUnexpectedMessage(
        this ILogger logger,
        LogLevel logLevel,
        IPgBackendMessage message);
}
