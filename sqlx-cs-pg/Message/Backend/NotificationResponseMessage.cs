using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class NotificationResponseMessage(int processId, string channelName, string payload) : IPgBackendMessage, IPgBackendMessageDecoder<NotificationResponseMessage>
{
    internal int ProcessId { get; } = processId;
    internal string ChannelName { get; } = channelName;
    internal string Payload { get; } = payload;
    
    public static NotificationResponseMessage Decode(ReadBuffer buffer)
    {
        return new NotificationResponseMessage(
            buffer.ReadInt(),
            buffer.ReadCString(),
            buffer.ReadCString());
    }
}
