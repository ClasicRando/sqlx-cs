using Microsoft.Extensions.Logging;
using Sqlx.Core.Connection;
using Sqlx.Core.Pool;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Pool;

public sealed partial class PgConnectionPool(PgConnectOptions options) : IConnectionPool
{
    private readonly ILogger<PgConnectionPool> _logger = options.LoggerFactory
        .CreateLogger<PgConnectionPool>();
    
    public PgConnectOptions ConnectOptions { get; } = options;
    
    public IConnection CreateConnection()
    {
        return new PgConnection(this);
    }

    internal Task<PgStream> AcquireStream()
    {
        return Task.FromResult(new PgStream(new AsyncStream(), this));
    }

    internal async ValueTask Return(PgStream stream, CancellationToken cancellationToken)
    {
        await stream.CleanUp(cancellationToken);
        stream.Dispose();
    }
    
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
