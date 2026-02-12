using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Sqlx.Core;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Pool;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Logging;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Backend.Information;
using Sqlx.Postgres.Pool;
using PgConnector = Sqlx.Postgres.Connector.PgConnector;

namespace Sqlx.Postgres.Notify;

internal sealed class PgListener : IPgListener
{
    private const string UnlistenAll = "UNLISTEN *;";
    private const string ListenersQuery = "SELECT * FROM pg_listening_channels();";
    private bool _disposed;
    private readonly List<string> _channels = [];
    private readonly PgConnectionPool _pool;
    private PgConnector? _pgConnector;
    private readonly ILogger<PgListener> _logger;

    internal PgListener(PgConnectionPool pool)
    {
        _pool = pool;
        _logger = pool.ConnectOptions.LoggerFactory.CreateLogger<PgListener>();
    }

    private ConnectionStatus Status => _pgConnector?.Status ?? ConnectionStatus.Closed;

    internal IReadOnlyList<string> Channels => _channels;

    public async Task ListenAsync(string channel, CancellationToken cancellationToken = default)
    {
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await _pgConnector!.SendQueryMessage(
                $"LISTEN {channel.QuoteIdentifier()};",
                cancellationToken)
            .ConfigureAwait(false);
        await _pgConnector.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
            .ConfigureAwait(false);
        _channels.Add(channel);
    }

    public async Task ListenAllAsync(
        IEnumerable<string> channels,
        CancellationToken cancellationToken = default)
    {
        var startIndex = _channels.Count;
        _channels.AddRange(channels);
        var query = BuildListenAllQuery(CollectionsMarshal.AsSpan(_channels)[startIndex..]);

        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        await _pgConnector!.SendQueryMessage(query, cancellationToken)
            .ConfigureAwait(false);
        await _pgConnector.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UnlistenAsync(string channel, CancellationToken cancellationToken = default)
    {
        _channels.Remove(channel);
        if (Status is ConnectionStatus.Broken or ConnectionStatus.Closed)
        {
            return;
        }

        await _pgConnector!.SendQueryMessage(
            $"UNLISTEN {channel.QuoteIdentifier()};",
            cancellationToken).ConfigureAwait(false);
        await _pgConnector.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UnlistenAllAsync(CancellationToken cancellationToken = default)
    {
        _channels.Clear();
        if (Status is ConnectionStatus.Broken or ConnectionStatus.Closed)
        {
            return;
        }

        await _pgConnector!.SendQueryMessage(UnlistenAll, cancellationToken).ConfigureAwait(false);
        await _pgConnector.WaitForOrThrowError<ReadyForQueryMessage>(cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<PgNotification> ReceiveNextAsync(
        CancellationToken cancellationToken = default)
    {
        if (Status is ConnectionStatus.Fetching)
        {
            throw new PgException(
                "Attempted to receive notification from listener already listening");
        }

        while (true)
        {
            Either<PgNotification, ErrorResponseMessage> nextMessage;
            try
            {
                nextMessage = await _pgConnector!.WaitForNotificationOrError(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (e is not (IOException or SqlxException)) throw;

                _logger.LogErrorWaitingForNotification();
                await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (nextMessage.IsLeft)
            {
                return nextMessage.Left;
            }

            ErrorResponseMessage errorResponseMessage = nextMessage.Right;
            if (errorResponseMessage.InformationResponse.Code.IsCriticalConnectionError)
            {
                throw new PgException(errorResponseMessage);
            }
        }
    }

    // ReSharper disable once IteratorNeverReturns
    public async IAsyncEnumerable<PgNotification> ReceiveNotificationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            yield return await ReceiveNextAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Attempt to connect/reconnect if the underlining connection is closed or broken. If the
    /// connection is closed or broken, a new stream will be acquired from the pool and any previous
    /// channels listened to will be resubscribed to.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    private async ValueTask ConnectIfClosed(CancellationToken cancellationToken)
    {
        CheckDisposed();
        if (Status is not (ConnectionStatus.Closed or ConnectionStatus.Broken))
        {
            return;
        }

        PgConnector connector =
            await _pool.AcquireStreamAsync(cancellationToken).ConfigureAwait(false);
        _pgConnector = connector;

        if (_channels.Count == 0)
        {
            return;
        }

        var query = BuildListenAllQuery(CollectionsMarshal.AsSpan(_channels));
        await _pgConnector.SendQueryMessage(query, cancellationToken).ConfigureAwait(false);
    }

    internal async IAsyncEnumerable<string> QueryChannels(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await ConnectIfClosed(cancellationToken).ConfigureAwait(false);
        var stream = _pgConnector!.SendSimpleQuery(ListenersQuery, cancellationToken);
        await foreach (var item in stream.ConfigureAwait(false))
        {
            if (item.IsLeft)
            {
                yield return item.Left.GetStringNotNull(0);
            }
        }
    }

    /// <summary>
    /// Combine all channel names into a single semicolon separated <c>LISTEN</c> query.
    /// </summary>
    /// <param name="channels">Channel names to listen to</param>
    /// <returns>A combined listen query for all channels</returns>
    private static string BuildListenAllQuery(ReadOnlySpan<string> channels)
    {
        var query = new StringBuilder();
        foreach (var channel in channels)
        {
            query.Append("LISTEN ");
            query.AppendQuotedIdentifier(channel);
            query.Append(';');
        }

        return query.ToString();
    }

    private void CheckDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        UnlistenAllAsync().GetAwaiter().GetResult();
        if (_pgConnector != null)
        {
            _pool.Return(_pgConnector);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await UnlistenAllAsync().ConfigureAwait(false);
        if (_pgConnector != null)
        {
            _pool.Return(_pgConnector);
        }
    }
}
