using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Message.Frontend;

internal class ParseMessage(
    string preparedStatementName,
    string query,
    IReadOnlyList<PgType> pgTypes) : IPgFrontendMessage
{
    internal string PreparedStatementName { get; } = preparedStatementName;
    internal string Query { get; } = query;
    internal IReadOnlyList<PgType> PgTypes { get; } = pgTypes;

    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Parse);
        buffer.WriteLengthPrefixed(
            true,
            buf =>
            {
                buf.WriteCString(PreparedStatementName);
                buf.WriteCString(Query);
                buf.WriteShort((short)PgTypes.Count);
                foreach (PgType pgType in PgTypes)
                {
                    buf.WriteInt(pgType.TypeOid);
                }
            });
    }
}
