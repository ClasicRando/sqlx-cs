using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgBool : IPgDbType<bool>
{
    public static void Encode(bool value, WriteBuffer buffer)
    {
        buffer.WriteByte((byte)(value ? 1 : 0));
    }

    public static bool DecodeBytes(PgBinaryValue value)
    {
        return value.Buffer.ReadByte() != 0;
    }

    public static bool DecodeText(PgTextValue value)
    {
        return value.Chars[0] switch
        {
            't' => true,
            'f' => false,
            _ => throw ColumnDecodeError.Create<bool>(value.ColumnMetadata),
        };
    }
    
    public static PgType DbType => PgType.Bool;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(bool value)
    {
        return DbType;
    }
}
