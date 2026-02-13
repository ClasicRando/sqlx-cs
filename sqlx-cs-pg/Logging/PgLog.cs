using Microsoft.Extensions.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Notify;
using Sqlx.Postgres.Result;
using PgConnector = Sqlx.Postgres.Connector.PgConnector;

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
        this ILogger<PgConnector> logger,
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
        this ILogger<PgConnector> logger,
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
        this ILogger<PgConnector> logger,
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
        this ILogger<PgConnector> logger,
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
    [LoggerMessage(
        Message =
            "Server does not support protocol version 3.0. Min Protocol Support: {MinProtocolVersion}, Unknown Options: {ProtocolOptionsNotRecognized}")]
    internal static partial void LogNegotiateProtocolVersion(
        this ILogger<PgConnector> logger,
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
        this ILogger<PgConnector> logger,
        LogLevel logLevel,
        IPgBackendMessage message);

    /// <summary>
    /// Log that an unexpected message was received from the database server. Include the message
    /// itself.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="message">Backend message received from the database server</param>
    [LoggerMessage(Message = "Ignoring {message} since it's not an error or the desired type")]
    internal static partial void LogIgnoreUnexpectedMessage(
        this ILogger<PgAsyncResultSet> logger,
        LogLevel logLevel,
        IPgBackendMessage message);

    /// <summary>
    /// Log that a connector received more <see cref="ReadyForQueryMessage"/>s than expected
    /// </summary>
    /// <param name="logger">The logger</param>
    [LoggerMessage(
        Message = "Received more ReadyForQuery messages than expected",
        Level = LogLevel.Warning)]
    internal static partial void LogReceivedMoreReadyForQueryThanExpected(
        this ILogger<PgConnector> logger);

    /// <summary>
    /// Log that a connector received a <see cref="ReadyForQueryMessage"/> indicating the current
    /// transaction failed
    /// </summary>
    /// <param name="logger">The logger</param>
    [LoggerMessage(
        Message = "Server reported failed transaction",
        Level = LogLevel.Warning)]
    internal static partial void LogServerReportedFailedTransaction(
        this ILogger<PgConnector> logger);

    /// <summary>
    /// Log that a connector caught an unexpected exception while disposing of a connector
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="cause">Cause of this log event</param>
    [LoggerMessage(
        Message = "Error closing connector",
        Level = LogLevel.Error)]
    internal static partial void LogErrorWhileClosingConnector(
        this ILogger<PgConnector> logger,
        Exception cause);

    /// <summary>
    /// Log that a connector receive a COPY FROM response message
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="response">Response message</param>
    [LoggerMessage(
        Message = "Received copy in response: {Response}",
        Level = LogLevel.Debug)]
    internal static partial void LogCopyInResponse(
        this ILogger<PgConnector> logger,
        CopyInResponseMessage response);

    /// <summary>
    /// Log that a connector receive a COPY TO response message
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="response">Response message</param>
    [LoggerMessage(
        Message = "Received copy out response: {Response}",
        Level = LogLevel.Debug)]
    internal static partial void LogCopyOutResponse(
        this ILogger<PgConnector> logger,
        CopyOutResponseMessage response);

    /// <summary>
    /// Log that a connector caught encountered an exception while sending a copy fail
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="cause">Cause of this log event</param>
    [LoggerMessage(
        Message = "Error sending copy fail message",
        Level = LogLevel.Warning)]
    internal static partial void LogErrorWhileSendingCopyFail(
        this ILogger<PgConnector> logger,
        Exception cause);

    /// <summary>
    /// Log that a listener received an error waiting for a notification
    /// </summary>
    /// <param name="logger">The logger</param>
    [LoggerMessage(
        Message = "Error receiving notification. Attempting reconnect.",
        Level = LogLevel.Warning)]
    internal static partial void LogErrorWaitingForNotification(this ILogger<PgListener> logger);
}
