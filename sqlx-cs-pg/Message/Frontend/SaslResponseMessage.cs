using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal sealed class SaslResponseMessage(string clientMessage) : IPgFrontendMessage
{
    internal string ClientMessage { get; } = clientMessage;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Password);
        buffer.WriteLengthPrefixed(
            true,
            buf => buf.WriteString(ClientMessage));
    }
}
