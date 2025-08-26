using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class ParameterDescriptionMessage(int[] parameterTypes) : IPgBackendMessage, IPgBackendMessageDecoder<ParameterDescriptionMessage>
{
    internal int[] ParameterTypes { get; } = parameterTypes;
    
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
