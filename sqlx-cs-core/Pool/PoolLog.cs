using Microsoft.Extensions.Logging;

namespace Sqlx.Core.Pool;

public static partial class PoolLog
{
    /// <summary>
    /// Log that a connection was found in a pool that exceeded the maximum allowed lifetime and
    /// must be closed.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="maxLifeTime">Max lifetime allowed by the pool</param>
    /// <param name="connectionId">Unique ID of the connection</param>
    [LoggerMessage(Message = "Stream exceeded the max lifetime of the pool as {maxLifetime}. Connection ID = {connectionId}")]
    internal static partial void LogConnectionExceededMaxLifeTime(
        this ILogger logger,
        LogLevel logLevel,
        TimeSpan maxLifeTime,
        Guid connectionId);
    
    /// <summary>
    /// Log that a connection was closed but an error occured during the closing
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="logLevel">Log level of the message</param>
    /// <param name="connectionId">Unique ID of the connection</param>
    /// <param name="exception">Error during connection closing</param>
    [LoggerMessage(Message = "Error occured while closing Connection ID = {connectionId}")]
    internal static partial void LogErrorClosingConnection(
        this ILogger logger,
        LogLevel logLevel,
        Guid connectionId,
        Exception exception);
}
