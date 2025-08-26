using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal class SyncMessage : IPgFrontendMessage
{
    private SyncMessage() {}
    
    internal static SyncMessage Instance { get; } = new();
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Sync);
        buffer.WriteInt(4);
    }
}
