using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent by the server to provide the types associated with a prepared statement. This
/// message will only be sent when the client asks the server to describe the prepared statement.
/// As of now, the parameter types are not used.
/// </summary>
internal record ParameterDescriptionMessage(PgOid[] ParameterTypes)
    : IPgBackendMessage, IPgBackendMessageDecoder<ParameterDescriptionMessage>
{
    public static ParameterDescriptionMessage Decode(ReadOnlySequence<byte> buffer)
    {
        var parameterCount = buffer.ReadShort();
        var parameterTypes = new PgOid[parameterCount];
        for (var i = 0; i < parameterCount; i++)
        {
            parameterTypes[i] = new PgOid(buffer.ReadUInt());
        }

        return new ParameterDescriptionMessage(parameterTypes);
    }
}
