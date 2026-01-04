using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Notify;

/// <summary>
/// Asynchronous notification message sent by the database backend. This message will only be sent
/// if the connection is listening to the <see cref="ChannelName"/> that is associated with the
/// notification.
/// </summary>
/// <param name="ProcessId">ID of the backend process that sent the notification</param>
/// <param name="ChannelName">Channel name of the notification</param>
/// <param name="Payload">Contents of the notification</param>
public record PgNotification(int ProcessId, string ChannelName, string Payload)
    : IPgBackendMessage, IPgBackendMessageDecoder<PgNotification>
{
    public static PgNotification Decode(ReadOnlySequence<byte> buffer)
    {
        return new PgNotification(buffer.ReadInt(), buffer.ReadCString(), buffer.ReadCString());
    }
}
