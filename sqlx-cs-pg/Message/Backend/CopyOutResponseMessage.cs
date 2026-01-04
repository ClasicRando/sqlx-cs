using System.Buffers;
using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// <para>
/// Message sent after initializing a copy operation using the <c>COPY TO</c> command. The client
/// will then receive zero or more <see cref="CopyDataMessage"/>s as part of the protocol.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-COPYOUTRESPONSE">docs</a>
/// </summary>
internal readonly struct CopyOutResponseMessage(CopyResponse Response)
    : IPgBackendMessage, IPgBackendMessageDecoder<CopyOutResponseMessage>
{
    public static CopyOutResponseMessage Decode(ReadOnlySequence<byte> buffer)
    {
        return new CopyOutResponseMessage(CopyResponse.Decode(buffer));
    }
}
