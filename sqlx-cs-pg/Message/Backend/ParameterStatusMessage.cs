using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class ParameterStatusMessage(string name, string value) : IPgBackendMessage, IPgBackendMessageDecoder<ParameterStatusMessage>
{
    internal string Name { get; } = name;
    internal string Value { get; } = value;
    
    public static ParameterStatusMessage Decode(ReadBuffer buffer)
    {
        return new ParameterStatusMessage(buffer.ReadCString(), buffer.ReadCString());
    }
}
