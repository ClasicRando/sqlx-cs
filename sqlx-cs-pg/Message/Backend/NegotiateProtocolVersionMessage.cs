using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent by the server to signal that the intended minor protocol version is too high or
/// unsupported protocol options were supplied. This message is always followed by an
/// <see cref="ErrorResponseMessage"/> since the login attempt failed.
/// </summary>
internal record NegotiateProtocolVersionMessage(
    int NewestMinorProtocolVersion,
    string[] ProtocolOptionsNotRecognized)
    : IPgBackendMessage, IPgBackendMessageDecoder<NegotiateProtocolVersionMessage>
{
    public static NegotiateProtocolVersionMessage Decode(ReadBuffer buffer)
    {
        var newestMinorProtocol = buffer.ReadInt();
        var count = buffer.ReadInt();
        var unrecognizedOptions = new string[count];
        for (var i = 0; i < count; i++)
        {
            unrecognizedOptions[i] = buffer.ReadCString();
        }

        return new NegotiateProtocolVersionMessage(newestMinorProtocol, unrecognizedOptions);
    }
}
