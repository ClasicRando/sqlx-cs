using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal class DescribeMessage(MessageTarget messageTarget, string name) : IPgFrontendMessage
{
    internal MessageTarget MessageTarget { get; } = messageTarget;
    internal string Name { get; } = name;

    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Describe);
        buffer.WriteLengthPrefixed(
            true,
            buf =>
            {
                buf.WriteByte((byte)MessageTarget);
                buf.WriteCString(Name);
            });
    }
}
