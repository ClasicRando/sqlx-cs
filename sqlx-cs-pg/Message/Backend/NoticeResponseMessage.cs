using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Backend.Information;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class NoticeResponseMessage(InformationResponse informationResponse) : IPgBackendMessage, IPgBackendMessageDecoder<NoticeResponseMessage>
{
    internal InformationResponse InformationResponse { get; } = informationResponse;

    public static NoticeResponseMessage Decode(ReadBuffer buffer)
    {
        return new NoticeResponseMessage(InformationResponse.Decode(buffer));
    }
}
