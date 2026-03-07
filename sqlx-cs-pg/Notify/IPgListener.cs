namespace Sqlx.Postgres.Notify;

/// <summary>
/// Database connection that only listens for notifications and does not provide any methods that
/// execute queries or any other operations. You can choose which channels it listens to and remove
/// channels as you want.
/// <h3>Disconnects</h3>
/// The listener will eagerly reconnect if needed to ensure that notifications can still be
/// streamed. Just note that while the connection has been disconnected, notifications are not
/// being polled or a recent notifications is being processed, notifications will be lost. This is
/// the nature of a non-persistent async notification system and should be seen as a tradeoff for a
/// simple system. In the case you need these notifications to be more durable, consider a messages
/// table or traditional message queue.
/// </summary>
public interface IPgListener : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Request listening to the specified channel. After completion, this listener will now receive
    /// async notifications from the server for this channel.
    /// </summary>
    /// <param name="channel">Channel name</param>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    Task ListenAsync(string channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request listening to all specified channels. After completion, this listener will now
    /// receive async notifications from the server for those channels.
    /// </summary>
    /// <param name="channels">Channel names</param>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    Task ListenAllAsync(
        IEnumerable<string> channels,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove specified channel from connection's listeners. After completion, this listener will
    /// no longer receive notifications for that channel.
    /// </summary>
    /// <param name="channel">Channel name</param>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    Task UnlistenAsync(string channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove all previously listened channels. After completion, this listener will no longer
    /// receive any notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    Task UnlistenAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Receive the next available notification from then database. This DOES NOT return the most
    /// recent notification sent but rather the next queued notification sent to the connection or
    /// the next connection received if there are no queued notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    /// <returns>Next available notification sent from the database</returns>
    ValueTask<PgNotification> ReceiveNextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a possibly infinite stream of notifications received by this listener. This will not
    /// return until the connection is broken or the cancellation token is cancelled.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operations</param>
    /// <returns>Async stream of notifications received from the database</returns>
    IAsyncEnumerable<PgNotification> ReceiveNotificationsAsync(
        CancellationToken cancellationToken = default);
}
