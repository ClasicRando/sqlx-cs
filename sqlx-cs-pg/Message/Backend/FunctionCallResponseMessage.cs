using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class FunctionCallResponseMessage(byte[]? data) : IPgBackendMessage, IPgBackendMessageDecoder<FunctionCallResponseMessage>
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly byte[]? _data = data;

    internal bool IsNull => _data is null;

    internal ReadOnlySpan<byte> Data => _data.AsSpan();
    
    public static FunctionCallResponseMessage Decode(ReadBuffer buffer)
    {
        var length = buffer.ReadInt();
        return length < 0
            ? new FunctionCallResponseMessage(null)
            : new FunctionCallResponseMessage(buffer.ReadBytes(length));
    }
}
