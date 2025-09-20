using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// <para>
/// Message sent to signal that a command was completed and no more rows for that command will be
/// sent. The actual message contains a CString describing the operations outcome but the number of
/// rows impacted is parsed from the message (if a count can be extracted).
/// </para>
/// <a href="https://www.postgresql.org/docs/current/protocol-message-formats.html#PROTOCOL-MESSAGE-FORMATS-COMMANDCOMPLETE">docs</a>
/// </summary>
internal sealed class CommandCompleteMessage(long rowCount, string message)
    : IPgBackendMessage, IPgBackendMessageDecoder<CommandCompleteMessage>
{
    internal long RowCount { get; } = rowCount;
    internal string Message { get; } = message;

    public static CommandCompleteMessage Decode(ReadBuffer buffer)
    {
        var message = buffer.ReadCString();
        var rowCount = ExtractRowCount(message);
        return new CommandCompleteMessage(rowCount, message);
    }

    /// <summary>
    /// Messages are in a format of a command keyword, followed by the rows count (except for INSERT
    /// which always has 0 before the row count) to extract the row count iterate backwards until
    /// we find a non-digit character and parse that span.
    /// </summary>
    /// <param name="message">Message to parse</param>
    /// <returns>Row count or -1 if parsing fails</returns>
    private static long ExtractRowCount(ReadOnlySpan<char> message)
    {
        var i = message.Length - 1;
        for (; i >= 0; i--)
        {
            if (char.IsDigit(message[i])) continue;
            
            i++;
            break;
        }

        return long.TryParse(message[i..], out var rowCount) ? rowCount : -1;
    }
}
