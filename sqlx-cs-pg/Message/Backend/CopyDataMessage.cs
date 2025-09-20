using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent from the server as the contents of a row copied from the server. Depending on the
/// copy format used to initialize the <c>COPY TO</c> action, the data might represent text or
/// binary encoded data.
/// </summary>
internal sealed class CopyDataMessage(byte[] data) : IPgBackendMessage, IPgBackendMessageDecoder<CopyDataMessage>
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly byte[] _data = data;

    private ReadOnlySpan<byte> Data => _data.AsSpan();

    public static CopyDataMessage Decode(ReadBuffer buffer)
    {
        return new CopyDataMessage(buffer.ReadBytes());
    }
}
