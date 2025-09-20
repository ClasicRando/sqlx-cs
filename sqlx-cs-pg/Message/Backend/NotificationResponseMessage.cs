using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent by the server when an asynchronous notification is sent. This message will only be
/// sent if the connection is listening to the <see cref="ChannelName"/> that is associated with the
/// notification.
/// </summary>
internal record NotificationResponseMessage(int ProcessId, string ChannelName, string Payload)
    : IPgBackendMessage, IPgBackendMessageDecoder<NotificationResponseMessage>
{
    public static NotificationResponseMessage Decode(ReadBuffer buffer)
    {
        return new NotificationResponseMessage(
            buffer.ReadInt(),
            buffer.ReadCString(),
            buffer.ReadCString());
    }
}
