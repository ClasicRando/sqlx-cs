using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgChar : IPgDbType<byte>
{
    public static void Encode(byte value, WriteBuffer buffer)
    {
        buffer.WriteByte(value);
    }

    public static byte DecodeBytes(PgBinaryValue value)
    {
        return value.Buffer.ReadByte();
    }

    public static byte DecodeText(PgTextValue value)
    {
        return value.Chars.Length switch
        {
            4 => (byte)(((byte)value.Chars[1] << 6) | ((byte)value.Chars[2] << 3) | (byte)value.Chars[3]),
            1 => (byte)value.Chars[0],
            0 => 0,
            _ => throw ColumnDecodeError.Create<byte>(
                value.ColumnMetadata,
                $"Received invalid \"char\" text, {value}"),
        };
    }
    
    public static PgType DbType => PgType.Char;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(byte value)
    {
        return DbType;
    }
}
