using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal class SaslInitialMessage(string mechanism, string saslData) : IPgFrontendMessage
{
    internal string Mechanism { get; } = mechanism;
    internal string SaslData { get; } = saslData;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Password);
        buffer.WriteLengthPrefixed(
            true,
            buf =>
            {
                buf.WriteCString(Mechanism);
                buf.WriteInt(SaslData.Length);
                buf.WriteString(SaslData);
            });
    }
}
