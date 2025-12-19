using Microsoft.Extensions.Logging;
using Sqlx.Core.Pool;
using Sqlx.Core.Stream;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Stream;

namespace Sqlx.Postgres.Pool;

public sealed partial class PgConnectionPool(PgConnectOptions options)
    : IConnectionPool<IPgConnection, IPgBindable, IPgExecutableQuery, IPgQueryBatch, IPgDataRow>
{
    private readonly ILogger<PgConnectionPool> _logger = options.LoggerFactory
        .CreateLogger<PgConnectionPool>();

    public PgConnectOptions ConnectOptions { get; } = options;

    public IPgConnection CreateConnection()
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
