using System.Buffers;
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
internal sealed class BackendDataKeyMessage(int processId, int secretKey)
    : IPgBackendMessage, IPgBackendMessageDecoder<BackendDataKeyMessage>
{
    internal int ProcessId { get; } = processId;
    internal int SecretKey { get; } = secretKey;

    public static BackendDataKeyMessage Decode(ReadOnlySequence<byte> buffer)
    {
        return new BackendDataKeyMessage(buffer.ReadInt(), buffer.ReadInt());
    }
}
