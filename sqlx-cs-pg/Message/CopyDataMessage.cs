using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Message;

internal sealed class CopyDataMessage(byte[] data) : IPgBackendMessage, IPgBackendMessageDecoder<CopyDataMessage>, IPgFrontendMessage
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private byte[] Data { get; } = data;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteByte((byte)PgFrontendMessageType.CopyData);
        buffer.WriteInt(Data.Length + 4);
        buffer.WriteBytes(Data.AsSpan());
    }

    public static CopyDataMessage Decode(ReadBuffer buffer)
    {
        return new CopyDataMessage(buffer.ReadBytes());
    }
}
