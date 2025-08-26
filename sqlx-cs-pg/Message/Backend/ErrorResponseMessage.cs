using Sqlx.Core.Buffer;
using Sqlx.Postgres.Message.Backend.Information;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class ErrorResponseMessage(InformationResponse informationResponse) : IPgBackendMessage, IPgBackendMessageDecoder<ErrorResponseMessage>
{
    internal InformationResponse InformationResponse { get; } = informationResponse;
    
    public static ErrorResponseMessage Decode(ReadBuffer buffer)
    {
        return new ErrorResponseMessage(Information.InformationResponse.Decode(buffer));
    }
}
