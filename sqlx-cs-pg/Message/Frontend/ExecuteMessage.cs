using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal class ExecuteMessage(string? portalName, int maxRowCount) : IPgFrontendMessage
{
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Execute);
        buffer.WriteLengthPrefixed(
            true,
            buf =>
            {
                buf.WriteCString(portalName ?? string.Empty);
                buf.WriteInt(maxRowCount);
            });
    }
}
