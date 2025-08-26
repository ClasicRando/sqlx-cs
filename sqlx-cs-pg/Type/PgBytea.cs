using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgBytea: IPgDbType<byte[]>
{
    public static void Encode(byte[] value, WriteBuffer buffer)
    {
        buffer.WriteBytes(value.AsSpan());
    }

    public static byte[] DecodeBytes(PgBinaryValue value)
    {
        return value.Buffer.ReadBytes();
    }

    public static byte[] DecodeText(PgTextValue value)
    {
        return value.Chars.StartsWith(HexStart)
            ? DecodeWithPrefix(value, value.ColumnMetadata)
            : DecodeWithoutPrefix(value);
    }
    
    public static PgType DbType => PgType.Bytea;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(byte[] value)
    {
        return DbType;
    }

    private const string HexStart = @"\x";

    private static byte[] DecodeWithPrefix(ReadOnlySpan<char> value, PgColumnMetadata metadata)
    {
        var size = value.Length - HexStart.Length;
        ColumnDecodeError.CheckOrThrow<byte[]>(
            (size & 0x01) == 0,
            metadata,
            () => "Hex encoded byte array must have an even number of elements");

        var index = HexStart.Length;
        var result = new byte[size >> 1];
        for (var i = 0; i < size; i++)
        {
            var currentByte = HexUtils.CharToDigit<byte[]>(value[index++], metadata) << 4;
            var other = HexUtils.CharToDigit<byte[]>(value[index++], metadata);
            result[i] = (byte)(currentByte | other);
        }
        return result;
    }

    private static byte[] DecodeWithoutPrefix(ReadOnlySpan<char> value)
    {
        var maxIndex = value.Length - 1;
        using var buffer = new WriteBuffer(maxIndex);
        var index = 0;

        while (index <= maxIndex)
        {
            var currentChar = value[index++];
            if (currentChar != '\\')
            {
                buffer.WriteByte((byte)currentChar);
                continue;
            }

            var nextChar = value[index++];
            if (nextChar == '\\')
            {
                buffer.WriteByte((byte)nextChar);
                continue;
            }

            var secondDigit = value[index++] - (byte)'0';
            var thirdDigit = value[index++] - (byte)'0';
            var result = thirdDigit + secondDigit * 8 + nextChar * 8 * 8;
            buffer.WriteByte((byte)result);
        }
        return buffer.CopyBytes();
    }
}
