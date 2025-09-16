using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Postgres.Auth.Sasl;

namespace Sqlx.Postgres.Connection;

/// <summary>
/// Connection options for a Postgresql database. Use this to construct a
/// <see cref="Sqlx.Postgres.Pool.PgConnectionPool"/>.
/// </summary>
public class PgConnectOptions
{
    /// <summary>
    /// Builder of a <see cref="PgConnectOptions"/>. Use this to assign all desired options and
    /// finalize by calling <see cref="Build"/> to construct your final connection options instance. 
    /// </summary>
    /// <param name="host">Host name/address of the Postgresql database</param>
    /// <param name="port">Host port to connect to. Postgresql's default port is 5432.</param>
    /// <param name="username"></param>
    public class Builder(string host, ushort port, string username)
    {
        private const string DefaultApplicationName = "sqlx-cs-driver";
        
        /// <summary>
        /// Host name/address of the Postgresql database
        /// </summary>
        public string Host { get; } = host;
        /// <summary>
        /// Port to connect to the host. Postgresql's default port is 5432.
        /// </summary>
        public ushort Port { get; } = port;
        /// <summary>
        /// Username to connect to the database
        /// </summary>
        public string Username { get; } = username;
        /// <summary>
        /// <c>application_name</c> property value supplied when creating the connection. By
        /// default, this is value is <see cref="DefaultApplicationName"/>
        /// </summary>
        public string ApplicationName { get; set; } = DefaultApplicationName;
        /// <summary>
        /// Optional password if the username requires it 
        /// </summary>
        public string? Password { get; set; }
        /// <summary>
        /// Optional database to connect to upon login. Defaults to user's default database.
        /// </summary>
        public string? Database { get; set; }
        /// <summary>
        /// Optional global query timeout. Default to 0 which disables query timeouts.
        /// </summary>
        public TimeSpan QueryTimeout { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Size of the prepared statement cache. Setting a larger size will allow for more
        /// statements to be executed without parsing again, but it will accumulate more statements
        /// on the server side which could impact performance of the server.
        /// </summary>
        public int StatementCacheCapacity { get; set; } = 100;
        /// <summary>
        /// True if the extended query protocol should be used for simple queries that:
        /// <list type="number">
        ///     <item>are a single statement</item>
        ///     <item>contain no parameter placeholder characters</item>
        /// </list>
        /// The extended query protocol allows for binary encoding of result rows which generally
        /// perform better than text based encoding.
        /// </summary>
        public bool UseExtendedProtocolForSimpleQueries { get; set; } = true;
        /// <summary>
        /// This parameter adjusts the number of digits used for textual output of floating-point
        /// values, including float4, float8, and geometric data types. Default is 1.
        /// <a href="https://www.postgresql.org/docs/16/runtime-config-client.html#GUC-EXTRA-FLOAT-DIGITS">docs</a>
        /// </summary>
        public int ExtraFloatPoints { get; set; } = 1;
        /// <summary>
        /// SSL mode of the connection. Defaults to <see cref="SslMode.Prefer"/>.
        /// </summary>
        public SslMode SslMode { get; set; } = SslMode.Prefer;
        /// <summary>
        /// The default schema within the database connection. Sets the <c>search_path</c>
        /// connection parameter. When null specified (the default) then the default connection
        /// property is used which is public.
        /// </summary>
        public string? CurrentSchema { get; set; } = null;
        /// <summary>
        /// 
        /// </summary>
        public ChannelBinding ChannelBinding { get; set; } = ChannelBinding.Prefer;
        /// <summary>
        /// Optional factory for creating <see cref="ILogger{TCategoryName}"/> instances. This is
        /// used by the connection pool and all objects created by the pool. If not specified by the
        /// builder, a default factory with a console logger will be used.
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; set; }

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
                $"{nameof(Host)}: {Host}, {nameof(Port)}: {Port}, {nameof(ApplicationName)}: {ApplicationName}, {nameof(Database)}: {Database}, {nameof(QueryTimeout)}: {QueryTimeout}, {nameof(StatementCacheCapacity)}: {StatementCacheCapacity}, {nameof(UseExtendedProtocolForSimpleQueries)}: {UseExtendedProtocolForSimpleQueries}, {nameof(ExtraFloatPoints)}: {ExtraFloatPoints}, {nameof(SslMode)}: {SslMode}, {nameof(CurrentSchema)}: {CurrentSchema}, {nameof(ChannelBinding)}: {ChannelBinding}";
        }
    }

    private PgConnectOptions(Builder builder)
    {
        Host = builder.Host;
        Port = builder.Port;
        Username = builder.Username;
        ApplicationName = builder.ApplicationName;
        Password = builder.Password;
        Database = builder.Database;
        QueryTimeout = builder.QueryTimeout;
        StatementCacheCapacity = builder.StatementCacheCapacity;
        UseExtendedProtocolForSimpleQueries = builder.UseExtendedProtocolForSimpleQueries;
        ExtraFloatPoints = builder.ExtraFloatPoints;
        SslMode = builder.SslMode;
        CurrentSchema = builder.CurrentSchema;
        ChannelBinding = builder.ChannelBinding;
        LoggerFactory = builder.LoggerFactory
                        ?? Microsoft.Extensions.Logging.LoggerFactory.Create(
                            loggingBuilder => loggingBuilder.AddConsole());
    }
    
    /// <inheritdoc cref="Builder.Host"/>
    public string Host { get; }
    /// <inheritdoc cref="Builder.Port"/>
    public ushort Port { get; }
    /// <inheritdoc cref="Builder.Username"/>
    public string Username { get; }
    /// <inheritdoc cref="Builder.ApplicationName"/>
    public string ApplicationName { get; }
    /// <inheritdoc cref="Builder.Password"/>
    public string? Password { get; }
    /// <inheritdoc cref="Builder.Database"/>
    public string? Database { get; }
    /// <inheritdoc cref="Builder.QueryTimeout"/>
    public TimeSpan QueryTimeout { get; }
    /// <inheritdoc cref="Builder.StatementCacheCapacity"/>
    public int StatementCacheCapacity { get; }
    /// <inheritdoc cref="Builder.UseExtendedProtocolForSimpleQueries"/>
    public bool UseExtendedProtocolForSimpleQueries { get; }
    /// <inheritdoc cref="Builder.ExtraFloatPoints"/>
    public int ExtraFloatPoints { get; }
    /// <inheritdoc cref="Builder.SslMode"/>
    public SslMode SslMode { get; }
    /// <inheritdoc cref="Builder.CurrentSchema"/>
    public string? CurrentSchema { get; }
    /// <inheritdoc cref="Builder.ChannelBinding"/>
    public ChannelBinding ChannelBinding { get; }
    /// <summary>
    /// <see cref="ILogger{TCategoryName}"/> factor used by the connection pool and all objects
    /// created by the pool. If not specified by the builder, a default factory with a console
    /// logger will be used.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }
    
    public override string ToString()
    {
        return
            $"{nameof(Host)}: {Host}, {nameof(Port)}: {Port}, {nameof(ApplicationName)}: {ApplicationName}, {nameof(Database)}: {Database}, {nameof(QueryTimeout)}: {QueryTimeout}, {nameof(StatementCacheCapacity)}: {StatementCacheCapacity}, {nameof(UseExtendedProtocolForSimpleQueries)}: {UseExtendedProtocolForSimpleQueries}, {nameof(ExtraFloatPoints)}: {ExtraFloatPoints}, {nameof(SslMode)}: {SslMode}, {nameof(CurrentSchema)}: {CurrentSchema}, {nameof(ChannelBinding)}: {ChannelBinding}";
    }
}
