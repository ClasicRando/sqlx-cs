using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent by the server to provide the types associated with a prepared statement. This
/// message will only be sent when the client asks the server to describe the prepared statement.
/// As of now, the parameter types are not used.
/// </summary>
internal record ParameterDescriptionMessage(int[] ParameterTypes)
    : IPgBackendMessage, IPgBackendMessageDecoder<ParameterDescriptionMessage>
{
    public static ParameterDescriptionMessage Decode(ReadBuffer buffer)
    {
        var parameterCount = buffer.ReadShort();
        var parameterTypes = new int[parameterCount];
        for (var i = 0; i < parameterCount; i++)
        {
            parameterTypes[i] = buffer.ReadInt();
        }

        return new ParameterDescriptionMessage(parameterTypes);
    }
}
