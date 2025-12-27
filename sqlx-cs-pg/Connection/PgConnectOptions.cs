using Microsoft.Extensions.Logging;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Auth.Sasl;

namespace Sqlx.Postgres.Connection;

/// <summary>
/// Connection options for a Postgresql database. Use this to construct a
/// <see cref="Sqlx.Postgres.Pool.PgConnectionPool"/>.
/// </summary>
public record PgConnectOptions
{
    private const string DefaultApplicationName = "sqlx-cs-driver";

    /// <summary>
    /// Host name/address of the Postgresql database
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Port to connect to the host. Postgresql's default port is 5432.
    /// </summary>
    public required ushort Port { get; init; }

    /// <summary>
    /// Username to connect to the database
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// <c>application_name</c> property value supplied when creating the connection. By
    /// default, this is value is <see cref="DefaultApplicationName"/>
    /// </summary>
    public string ApplicationName { get; init; } = DefaultApplicationName;

    /// <summary>
    /// Optional password if the username requires it 
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Optional database to connect to upon login. Defaults to user's default database.
    /// </summary>
    public string? Database { get; init; }

    /// <summary>
    /// Optional global connect timeout. Values less than or equal to zero are treated as a
    /// disabled. Default to 15 seconds.
    /// <see cref="Timeout.InfiniteTimeSpan"/>
    /// </summary>
    public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Optional global query timeout. Values less than or equal to zero are treated as a disabled
    /// timeout. Default is disabled.
    /// </summary>
    public TimeSpan QueryTimeout { get; init; } = Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Size of the prepared statement cache. Setting a larger size will allow for more
    /// statements to be executed without parsing again, but it will accumulate more statements
    /// on the server side which could impact performance of the server.
    /// </summary>
    public int StatementCacheCapacity { get; init; } = 100;

    /// <summary>
    /// True if the extended query protocol should be used for simple queries that:
    /// <list type="number">
    ///     <item>are a single statement</item>
    ///     <item>contain no parameter placeholder characters</item>
    /// </list>
    /// The extended query protocol allows for binary encoding of result rows which generally
    /// perform better than text based encoding.
    /// </summary>
    public bool UseExtendedProtocolForSimpleQueries { get; init; } = true;

    /// <summary>
    /// This parameter adjusts the number of digits used for textual output of floating-point
    /// values, including float4, float8, and geometric data types. Default is 1.
    /// <a href="https://www.postgresql.org/docs/16/runtime-config-client.html#GUC-EXTRA-FLOAT-DIGITS">docs</a>
    /// </summary>
    public int ExtraFloatPoints { get; init; } = 1;

    public SslMode SslMode { get; init; } = SslMode.Prefer;

    /// <summary>
    /// The default schema within the database connection. Sets the <c>search_path</c>
    /// connection parameter. When null specified (the default) then the default connection
    /// property is used which is public.
    /// </summary>
    public string? CurrentSchema { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public ChannelBinding ChannelBinding { get; init; } = ChannelBinding.Prefer;

    /// <summary>
    /// <see cref="ILogger{TCategoryName}"/> factor used by the connection pool and all objects
    /// created by the pool. If not specified by the builder, a default factory with a console
    /// logger will be used.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; init; } =
        Microsoft.Extensions.Logging.LoggerFactory.Create(loggingBuilder =>
            loggingBuilder.AddConsole());
}
