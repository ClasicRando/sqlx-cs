using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal class CloseMessage(MessageTarget messageTarget, string? targetName) : IPgFrontendMessage
{
    internal MessageTarget MessageTarget { get; } = messageTarget;
    internal string? TargetName { get; } = targetName;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Close);
        buffer.WriteLengthPrefixed(
            true,
            buf =>
            {
                buf.WriteByte((byte)MessageTarget);
                buf.WriteCString(TargetName ?? string.Empty);
            });
    }
}
