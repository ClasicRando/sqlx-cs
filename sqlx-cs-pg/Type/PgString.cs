using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgString : IPgDbType<string>
{
    public static void Encode(string value, WriteBuffer buffer)
    {
        buffer.WriteString(value);
    }

    public static string DecodeBytes(PgBinaryValue value)
    {
        return value.Buffer.ReadText();
    }

    public static string DecodeText(PgTextValue value)
    {
        return new string(value.Chars);
    }
    
    public static PgType DbType => PgType.Text;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid
               || dbType.TypeOid == PgType.Varchar.TypeOid
               || dbType.TypeOid == PgType.Xml.TypeOid
               || dbType.TypeOid == PgType.Name.TypeOid
               || dbType.TypeOid == PgType.Bpchar.TypeOid;
    }

    public static PgType GetActualType(string value)
    {
        return DbType;
    }
}
