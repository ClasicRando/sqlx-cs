using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class BackendDataKeyMessage(int processId, int secretKey) : IPgBackendMessage, IPgBackendMessageDecoder<BackendDataKeyMessage>
{
    internal int ProcessId { get; } = processId;
    internal int SecretKey { get; } = secretKey;

    public static BackendDataKeyMessage Decode(ReadBuffer buffer)
    {
        return new BackendDataKeyMessage(buffer.ReadInt(), buffer.ReadInt());
    }
}
