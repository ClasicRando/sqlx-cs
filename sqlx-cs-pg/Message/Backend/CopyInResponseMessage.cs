using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class CopyInResponseMessage(CopyResponse copyResponse) : IPgBackendMessage, IPgBackendMessageDecoder<CopyInResponseMessage>
{
    internal CopyResponse CopyResponse { get; } = copyResponse;
    
    public static CopyInResponseMessage Decode(ReadBuffer buffer)
    {
        return new CopyInResponseMessage(CopyResponse.Decode(buffer));
    }
}
