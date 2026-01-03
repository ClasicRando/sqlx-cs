using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Buffer;

internal static class WriteBufferExtensions
{
    extension(IBufferWriter<byte> writeBuffer)
    {
        /// <summary>
        /// Write the <see cref="PgFrontendMessageType"/> code to the buffer as a <see cref="byte"/>
        /// </summary>
        /// <param name="code">Frontend message code</param>
        internal void WriteCode(PgFrontendMessageType code)
        {
            writeBuffer.WriteByte((byte)code);
        }
    }
}
