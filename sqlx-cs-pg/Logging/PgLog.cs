using Microsoft.Extensions.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Notify;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Logging;

/// <summary>
/// Logging class for source generating performant log methods
/// </summary>
internal static partial class PgLog
{
    /// <summary>
    /// Log a <c>NOTICE</c> message sent from the database
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="noticeResponse">Notice message</param>
    [LoggerMessage(Message = "Notice -> {@NoticeResponse}")]
    internal static partial void LogNotice(
        this ILogger<PgStream> logger,
        LogLevel logLevel,
        NoticeResponseMessage noticeResponse);
    
    /// <summary>
    /// Log a <see cref="PgNotification"/> message sent from the database. These are only received
    /// from active connections that have listened but is not used by a <see cref="PgListener"/>.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="notification">Notification message</param>
    [LoggerMessage(Message = "Notification -> {@notification}")]
    internal static partial void LogNotification(
        this ILogger<PgStream> logger,
        LogLevel logLevel,
        PgNotification notification);
    
    /// <summary>
    /// Log a parameter status update message sent from the database
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="name">Parameter's name</param>
    [LoggerMessage(Message = "Parameter updated. Name: {Name}")]
    internal static partial void LogParameterStatus(
        this ILogger<PgStream> logger,
        LogLevel logLevel,
        string name);

    /// <summary>
    /// Log that a backend key data message was sent. Only include the process ID not the secret.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="processId">Process ID that sent the message</param>
    [LoggerMessage(Message = "Got backend key data. Process ID: {ProcessId}")]
    internal static partial void LogBackendDataKey(
        this ILogger<PgStream> logger,
        LogLevel logLevel,
        int processId);
    
    /// <summary>
    /// Log that a message was sent to negotiate the protocol version used. This driver only
    /// supports protocol version 3.0 and later.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="minProtocolVersion">Minimum protocol required by the server</param>
    /// <param name="protocolOptionsNotRecognized">
    /// Protocol options not recognized by the server
    /// </param>
    [LoggerMessage(Message = "Server does not support protocol version 3.0. Min Protocol Support: {MinProtocolVersion}, Unknown Options: {ProtocolOptionsNotRecognized}")]
    internal static partial void LogNegotiateProtocolVersion(
        this ILogger<PgStream> logger,
        LogLevel logLevel,
        int minProtocolVersion,
        string[] protocolOptionsNotRecognized);
    
    /// <summary>
    /// Log that an unexpected message was received from the database server. Include the message
    /// itself.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="message">Backend message received from the database server</param>
    [LoggerMessage(Message = "Ignoring {message} since it's not an error or the desired type")]
    internal static partial void LogIgnoreUnexpectedMessage(
        this ILogger<PgStream> logger,
        LogLevel logLevel,
        IPgBackendMessage message);
}
