using Microsoft.Extensions.Logging;

namespace Sqlx.Core.Config;

public static class SqlxConfig
{
    public static LogLevel DetailedLoggingLevel { get; private set; } = LogLevel.Trace;
}
