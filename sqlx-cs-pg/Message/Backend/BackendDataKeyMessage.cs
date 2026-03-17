using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent after a successful login. Contents are:
/// <list type="number">
///     <item>the process ID of the backend receiving messages from this connection</item>
///     <item>the secret key of the backend to allow for query cancellation</item>
/// </list>
/// <a href="https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-BACKENDKEYDATA">docs</a>
/// </summary>
internal readonly record struct BackendDataKeyMessage(int ProcessId, int SecretKey)
    : IPgBackendMessage, IPgBackendMessageDecoder<BackendDataKeyMessage>
{
    public static PgBackendMessageType MessageType => PgBackendMessageType.BackendDataKey;

    public override string ToString()
    {
        return $"{nameof(BackendDataKeyMessage)}({nameof(ProcessId)}={ProcessId})";
    }

    public static BackendDataKeyMessage Decode(ReadOnlySpan<byte> buffer)
    {
        return new BackendDataKeyMessage(buffer.ReadInt(), buffer.ReadInt());
    }
}
