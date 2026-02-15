using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent during client initialization and after a successful login to let the client know
/// the status of certain key parameters such as client encoding and date style. This message is
/// also sent asynchronously whenever the client issues a <c>SET</c> command.
/// </summary>
internal record ParameterStatusMessage(string Name, string Value)
    : IPgBackendMessage, IPgBackendMessageDecoder<ParameterStatusMessage>
{
    public static PgBackendMessageType MessageType => PgBackendMessageType.ParameterStatus;

    public static ParameterStatusMessage Decode(ReadOnlySpan<byte> buffer)
    {
        return new ParameterStatusMessage(buffer.ReadCString(), buffer.ReadCString());
    }
}
