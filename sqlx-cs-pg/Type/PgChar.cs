using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgChar : IPgDbType<sbyte>, IHasArrayType
{
    public static void Encode(sbyte value, WriteBuffer buffer)
    {
        buffer.WriteByte((byte)value);
    }

    public static sbyte DecodeBytes(PgBinaryValue value)
    {
        return (sbyte)value.Buffer.ReadByte();
    }

    public static sbyte DecodeText(PgTextValue value)
    {
        return value.Chars.Length switch
        {
            4 => (sbyte)(((byte)value.Chars[1] << 6) | ((byte)value.Chars[2] << 3) | (byte)value.Chars[3]),
            1 => (sbyte)value.Chars[0],
            0 => 0,
            _ => throw ColumnDecodeException.Create<byte>(
                value.ColumnMetadata,
                $"Received invalid \"char\" text, {value}"),
        };
    }
    
    public static PgType DbType => PgType.Char;

    public static PgType ArrayDbType => PgType.CharArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(sbyte value)
    {
        return DbType;
    }
}
