using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Buffer;

internal static class WriteBufferExtensions
{
    /// <summary>
    /// Write the <see cref="PgFrontendMessageType"/> code to the buffer as a <see cref="byte"/>
    /// </summary>
    /// <param name="writeBuffer">the buffer to write to</param>
    /// <param name="code">frontend message code</param>
    internal static void WriteCode(this WriteBuffer writeBuffer, PgFrontendMessageType code)
    {
        writeBuffer.WriteByte((byte)code);
    }
}
