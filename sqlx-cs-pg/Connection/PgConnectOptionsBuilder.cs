using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Postgres.Auth.Sasl;

namespace Sqlx.Postgres.Connection;

public class PgConnectOptionsBuilder(string host, ushort port, string username)
{
    public string Host { get; } = host;
    public ushort Port { get; } = port;
    public string Username { get; } = username;
    public string ApplicationName { get; set; } = "sqlx-cs-driver";
    public string? Password { get; set; }
    public string? Database { get; set; }
    public TimeSpan QueryTimeout { get; set; } = TimeSpan.Zero;
    public int StatementCacheCapacity { get; set; } = 100;
    public bool UseExtendedProtocolForSimpleQueries { get; set; } = true;
    public int ExtraFloatPoints { get; set; } = 1;
    public SslMode SslMode { get; set; } = SslMode.Prefer;
    public string? CurrentSchema { get; set; } = null;
    public TimeSpan? TimeZoneOffset { get; set; } = null;
    public ChannelBinding ChannelBinding { get; set; } = ChannelBinding.Prefer;
    public ILoggerFactory LoggerFactory { get; set; } = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());

    public PgConnectOptions Build()
    {
        if (QueryTimeout < TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Timeout must be positive or 0 TimeSpan",
                nameof(QueryTimeout));
        }
        return new PgConnectOptions(this);
    }
    
    public override string ToString()
    {
        return
            $"{nameof(Host)}: {Host}, {nameof(Port)}: {Port}, {nameof(ApplicationName)}: {ApplicationName}, {nameof(Database)}: {Database}, {nameof(QueryTimeout)}: {QueryTimeout}, {nameof(StatementCacheCapacity)}: {StatementCacheCapacity}, {nameof(UseExtendedProtocolForSimpleQueries)}: {UseExtendedProtocolForSimpleQueries}, {nameof(ExtraFloatPoints)}: {ExtraFloatPoints}, {nameof(SslMode)}: {SslMode}, {nameof(CurrentSchema)}: {CurrentSchema}, {nameof(TimeZoneOffset)}: {TimeZoneOffset}, {nameof(ChannelBinding)}: {ChannelBinding}";
    }
}
