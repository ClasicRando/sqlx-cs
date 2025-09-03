using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Backend;

namespace Sqlx.Postgres.Message;

internal sealed class CopyDataMessage(byte[] data) : IPgBackendMessage, IPgBackendMessageDecoder<CopyDataMessage>
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private byte[] Data { get; } = data;

    public static CopyDataMessage Decode(ReadBuffer buffer)
    {
        return new CopyDataMessage(buffer.ReadBytes());
    }
}
