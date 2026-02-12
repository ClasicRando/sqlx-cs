using System.Diagnostics.CodeAnalysis;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// <para>
/// Message sent after initializing a copy operation using the <c>COPY FROM</c> command. The client
/// will then send zero or more <see cref="CopyDataMessage"/>s as part of the protocol.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-COPYINRESPONSE">docs</a>
/// </summary>
[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
internal record CopyInResponseMessage(CopyResponse CopyResponse)
    : IPgBackendMessage, IPgBackendMessageDecoder<CopyInResponseMessage>
{
    public static CopyInResponseMessage Decode(ReadOnlySpan<byte> buffer)
    {
        return new CopyInResponseMessage(CopyResponse.Decode(buffer));
    }
}
