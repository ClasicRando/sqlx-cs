using JetBrains.Annotations;
using Sqlx.Core.Query;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Fixtures;

namespace Sqlx.Postgres.Notify;

[ClassDataSource<DatabaseFixture>(Shared = SharedType.PerClass)]
[TestSubject(typeof(PgListener))]
public class PgListenerTest(DatabaseFixture databaseFixture)
{
    [Test]
    public async Task ListenAsyncAndUnlistenAsync_Should_AddToAndRemoveFromConnectionsListeners(
        CancellationToken ct)
    {
        const string channel = "test";
        await using IPgListener listener = databaseFixture.BasicPool.CreateListener();
        var pgListener = (PgListener)listener;

        var existingChannels = await pgListener.QueryChannels(ct).ToListAsync(ct);
        await Assert.That(existingChannels).DoesNotContain(channel);
        await Assert.That(pgListener.Channels).DoesNotContain(channel);

        await listener.ListenAsync(channel, ct);

        var postListenChannels = await pgListener.QueryChannels(ct).ToListAsync(ct);
        await Assert.That(postListenChannels).Contains(channel);
        await Assert.That(pgListener.Channels).Contains(channel);

        await listener.UnlistenAsync(channel, ct);

        var finalChannels = await pgListener.QueryChannels(ct).ToListAsync(ct);
        await Assert.That(finalChannels).DoesNotContain(channel);
        await Assert.That(pgListener.Channels).DoesNotContain(channel);
    }

    [Test]
    public async Task
        ListenAllAsyncAndUnlistenAllAsync_Should_AddAllAndRemoveAllConnectionsListeners(
            CancellationToken ct)
    {
        const string channel1 = "test1";
        const string channel2 = "test2";
        await using IPgListener listener = databaseFixture.BasicPool.CreateListener();
        var pgListener = (PgListener)listener;

        var existingChannels = await pgListener.QueryChannels(ct).ToListAsync(ct);
        await Assert.That(existingChannels).DoesNotContain(channel1).And.DoesNotContain(channel2);
        await Assert.That(pgListener.Channels).DoesNotContain(channel1).And
            .DoesNotContain(channel2);

        await listener.ListenAllAsync([channel1, channel2], ct);

        var postListenChannels = await pgListener.QueryChannels(ct).ToListAsync(ct);
        await Assert.That(postListenChannels).Contains(channel1).And.Contains(channel2);
        await Assert.That(pgListener.Channels).Contains(channel1).And.Contains(channel2);

        await listener.UnlistenAllAsync(ct);

        var finalChannels = await pgListener.QueryChannels(ct).ToListAsync(ct);
        await Assert.That(finalChannels).IsEmpty();
        await Assert.That(pgListener.Channels).IsEmpty();
    }

    [Test]
    public async Task ReceiveNextAsync_Should_ReceiveNotification(CancellationToken ct)
    {
        const string channel = "ReceiveNextAsync";
        const string notifyQuery = $"SELECT pg_notify('{channel}', '{channel}');";
        await using IPgListener listener = databaseFixture.BasicPool.CreateListener();
        using IPgConnection connection = databaseFixture.BasicPool.CreateConnection();

        await listener.ListenAsync(channel, ct);
        await connection.ExecuteNonQueryAsync(notifyQuery, ct);

        PgNotification notification = await listener.ReceiveNextAsync(ct);

        await Assert.That(notification).Member(n => n.ChannelName, cn => cn.IsEqualTo(channel));
        await Assert.That(notification).Member(n => n.Payload, cn => cn.IsEqualTo(channel));
    }


    [Test]
    public async Task ReceiveNotificationsAsync_Should_KeepWaitingForNotifications(
        CancellationToken ct)
    {
        const string channel = "ReceiveNotificationsAsync";
        const int maxNotificationCount = 10;
        await using IPgListener listener = databaseFixture.BasicPool.CreateListener();
        IPgConnection connection = databaseFixture.BasicPool.CreateConnection();


        await listener.ListenAsync(channel, ct);
        Task notificationTask = Task.Run(
            async () =>
            {
                using IPgConnection _ = connection;
                for (var j = 0; j < maxNotificationCount; j++)
                {
                    await connection.ExecuteNonQueryAsync($"SELECT pg_notify('{channel}', '{j}');", ct);
                }
            },
            ct);

        var notifications = await listener.ReceiveNotificationsAsync(ct)
            .Take(maxNotificationCount)
            .ToListAsync(ct);
        await notificationTask;

        for (var i = 0; i < notifications.Count; i++)
        {
            PgNotification notification = notifications[i];
            await Assert.That(notification).Member(n => n.ChannelName, cn => cn.IsEqualTo(channel));
            await Assert.That(notification).Member(
                n => n.Payload,
                cn => cn.IsEqualTo(i.ToString()));
        }
    }
}
