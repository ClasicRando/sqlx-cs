using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal sealed class BindMessage(
    string? portal,
    string statementName,
    short argumentsCount,
    ReadOnlyMemory<byte> arguments) : IPgFrontendMessage
{
    private string? Portal { get; } = portal;
    private string StatementName { get; } = statementName;
    private short ArgumentsCount { get; } = argumentsCount;
    private ReadOnlyMemory<byte> Arguments { get; } = arguments;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteByte((byte)PgFrontendMessageType.Bind);
        buffer.WriteLengthPrefixed(
            includeLength: true,
            buf =>
            {
                buf.WriteCString(Portal ?? "");
                buf.WriteCString(StatementName);
                buf.WriteShort(1);
                buf.WriteShort(1);
                buf.WriteShort(ArgumentsCount);
                buf.WriteBytes(Arguments);
                buf.WriteShort(1);
                buf.WriteShort(1);
            });
    }
}
