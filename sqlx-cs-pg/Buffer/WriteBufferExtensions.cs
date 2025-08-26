using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Buffer;

internal static class WriteBufferExtensions
{
    internal static void WriteCode(this WriteBuffer writeBuffer, PgFrontendMessageType code)
    {
        writeBuffer.WriteByte((byte)code);
    }
}
