using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Postgres.Auth.Sasl;

namespace Sqlx.Postgres.Connection;

public class PgConnectOptions(PgConnectOptionsBuilder builder)
{
    public string Host { get; } = builder.Host;
    public ushort Port { get; } = builder.Port;
    public string Username { get; } = builder.Username;
    public string ApplicationName { get; } = builder.ApplicationName;
    public string? Password { get; } = builder.Password;
    public string? Database { get; } = builder.Database;
    public TimeSpan QueryTimeout { get; } = builder.QueryTimeout;
    public int StatementCacheCapacity { get; } = builder.StatementCacheCapacity;
    public bool UseExtendedProtocolForSimpleQueries { get; } = builder.UseExtendedProtocolForSimpleQueries;
    public int ExtraFloatPoints { get; } = builder.ExtraFloatPoints;
    public SslMode SslMode { get; } = builder.SslMode;
    public string? CurrentSchema { get; } = builder.CurrentSchema;
    public TimeSpan? TimeZoneOffset { get; } = builder.TimeZoneOffset;
    public ChannelBinding ChannelBinding { get; } = builder.ChannelBinding;
    public ILoggerFactory LoggerFactory { get; } = builder.LoggerFactory;
    
    public override string ToString()
    {
        return
            $"{nameof(Host)}: {Host}, {nameof(Port)}: {Port}, {nameof(ApplicationName)}: {ApplicationName}, {nameof(Database)}: {Database}, {nameof(QueryTimeout)}: {QueryTimeout}, {nameof(StatementCacheCapacity)}: {StatementCacheCapacity}, {nameof(UseExtendedProtocolForSimpleQueries)}: {UseExtendedProtocolForSimpleQueries}, {nameof(ExtraFloatPoints)}: {ExtraFloatPoints}, {nameof(SslMode)}: {SslMode}, {nameof(CurrentSchema)}: {CurrentSchema}, {nameof(TimeZoneOffset)}: {TimeZoneOffset}, {nameof(ChannelBinding)}: {ChannelBinding}";
    }
}
