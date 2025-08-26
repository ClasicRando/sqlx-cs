using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal class QueryMessage(string sql) : IPgFrontendMessage
{
    internal string Sql { get; } = sql;
    
    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Query);
        buffer.WriteLengthPrefixed(
            true,
            buf => buf.WriteString(Sql));
    }
}
