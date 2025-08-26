using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class CopyOutResponseMessage(CopyResponse copyResponse) : IPgBackendMessage, IPgBackendMessageDecoder<CopyOutResponseMessage>
{
    internal CopyResponse CopyResponse { get; } = copyResponse;
    
    public static CopyOutResponseMessage Decode(ReadBuffer buffer)
    {
        return new CopyOutResponseMessage(CopyResponse.Decode(buffer));
    }
}
