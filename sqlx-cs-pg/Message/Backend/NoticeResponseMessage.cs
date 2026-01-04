using System.Buffers;
using Sqlx.Postgres.Message.Backend.Information;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// <para>
/// Message sent when the backend encounters an error either in its internal process or as a result
/// of a message passed from the frontend. The contents of the message is a
/// <see cref="Sqlx.Postgres.Message.Backend.Information.InformationResponse"/> packet.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-ERRORRESPONSE">docs</a>
/// </summary>
internal record NoticeResponseMessage(InformationResponse InformationResponse) : IPgBackendMessage, IPgBackendMessageDecoder<NoticeResponseMessage>
{
    public static NoticeResponseMessage Decode(ReadOnlySequence<byte> buffer)
    {
        return new NoticeResponseMessage(InformationResponse.Decode(buffer));
    }
}
