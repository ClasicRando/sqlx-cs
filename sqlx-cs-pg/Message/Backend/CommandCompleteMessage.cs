using Sqlx.Core.Buffer;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class CommandCompleteMessage(long rowCount, string message) : IPgBackendMessage, IPgBackendMessageDecoder<CommandCompleteMessage>
{
    internal long RowCount { get; } = rowCount;
    internal string Message { get; } = message;

    public static CommandCompleteMessage Decode(ReadBuffer buffer)
    {
        var message = buffer.ReadCString();
        var words = message.Split(' ');
        var rowCount = ExtractRowCount(words);
        return new CommandCompleteMessage(rowCount, message);
    }

    private static long ExtractRowCount(string[] words)
    {
        if (words.Length <= 1)
        {
            return 0;
        }

        long rowCount;
        if (words[0] != "INSERT")
        {
            return long.TryParse(words[1], out rowCount) ? 0 : rowCount;
        }
        
        if (words.Length < 3 || long.TryParse(words[2], out rowCount))
        {
            return 0;
        }
        return rowCount;

    }
}
