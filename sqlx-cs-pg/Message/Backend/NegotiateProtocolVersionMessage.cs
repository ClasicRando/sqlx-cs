using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class NegotiateProtocolVersionMessage(int minProtocolVersion, string[] protocolOptionsNotRecognized) : IPgBackendMessage, IPgBackendMessageDecoder<NegotiateProtocolVersionMessage>
{
    internal int MinProtocolVersion { get; } = minProtocolVersion;
    internal string[] ProtocolOptionsNotRecognized { get; } = protocolOptionsNotRecognized;
    
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
