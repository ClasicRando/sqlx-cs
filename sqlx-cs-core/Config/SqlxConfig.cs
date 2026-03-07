using Microsoft.Extensions.Logging;

namespace Sqlx.Core.Config;

/// <summary>
/// SQLx library global configuration properties
/// </summary>
public static class SqlxConfig
{
    /// <summary>
    /// Log level used by detailed statements usually emitted by driver code for events that aren't
    /// critical for regular operation. These events contain a lot more detail about internal driver
    /// activities and should only be turned on when absolutely needed, or you are trying to track
    /// down an issue. DO NOT TURN THIS ON IN PRODUCTION since it will almost certainly overload
    /// whatever log writing system you are using and/or generate a lot of noise in your logs.
    /// </summary>
    public static LogLevel DetailedLoggingLevel { get; set; } = LogLevel.None;
}
