using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// <see cref="IPgDbType{T}"/> for an array of <see cref="byte"/> values. Maps to the <c>BYTEA</c>
/// type.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-binary.html">docs</a>
/// </summary>
public abstract class PgBytea : IPgDbType<byte[]>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Simply writes all bytes in the array to the buffer
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/varlena.c#L471">pg source code</a>
    /// </summary>
    public static void Encode(byte[] value, IBufferWriter<byte> buffer)
    {
        buffer.Write(value.AsSpan());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Reads all available bytes in the value's buffer
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/varlena.c#L490">pg source code</a>
    /// </summary>
    public static byte[] DecodeBytes(in PgBinaryValue value)
    {
        return value.Buffer.ToArray();
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Decode the characters as either a prefixed hex format value (using
    /// <see cref="DecodeWithPrefix"/>) or an escape format value (using
    /// <see cref="DecodeWithoutPrefix"/>).
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/varlena.c#L388">pg source code</a>
    /// </summary>
    public static byte[] DecodeText(in PgTextValue value)
    {
        return value.Chars.StartsWith(HexStart)
            ? DecodeWithPrefix(value.Chars, value.ColumnMetadata)
            : DecodeWithoutPrefix(value.Chars);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Bytea;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.ByteaArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    private const string HexStart = @"\x";

    /// <summary>
    /// Decode the value into a <see cref="byte"/> array, interpreting value as a hex formatted
    /// bytea. This reads the span 2 characters at a time, combining each pair of characters into a
    /// single <see cref="byte"/>. The first character of each pair is converted to an
    /// <see cref="int"/> and put into the 4 left most bits. The second character is converted to an
    /// <see cref="int"/> and put into the 4 right most bits. Each pair is then packed into the
    /// resulting array.
    /// </summary>
    /// <param name="value">Span of hex encoded characters</param>
    /// <param name="metadata">Column metadata</param>
    /// <returns>A byte array that corresponds to the hex string</returns>
    private static byte[] DecodeWithPrefix(ReadOnlySpan<char> value, in PgColumnMetadata metadata)
    {
        var hexCharCount = value.Length - HexStart.Length;
        if ((hexCharCount & 0x01) != 0)
        {
            throw ColumnDecodeException.Create<byte[], PgColumnMetadata>(
                metadata,
                "Hex encoded byte array must have an even number of elements");
        }

        var size = hexCharCount >> 1;
        var index = HexStart.Length;
        var result = new byte[size];
        for (var i = 0; i < size; i++)
        {
            var currentByte = HexUtils.CharToDigit<byte[]>(value[index++], metadata) << 4;
            var other = HexUtils.CharToDigit<byte[]>(value[index++], metadata);
            result[i] = (byte)(currentByte | other);
        }

        return result;
    }

    /// <summary>
    /// <para>
    /// Decode the value into a ByteArray, interpreting value as an escape formatted bytea.
    /// </para>
    /// <para>
    /// This reads the value character by character, interpreting each character as a
    /// <see cref="byte"/> unless the character is a forward slash. In that case, it is checked if
    /// the slash is escaping a literal slash, or it means that the next 3 digits need to be
    /// interpreted as a combined hexadecimal <see cref="byte"/> value in the format of
    /// <c>x{first}{second}{third}</c>.
    /// </para>
    /// </summary>
    /// <param name="value">Span of hex encoded characters</param>
    /// <returns>A byte array that corresponds to the hex string</returns>
    private static byte[] DecodeWithoutPrefix(ReadOnlySpan<char> value)
    {
        var maxIndex = value.Length - 1;
        using var buffer = new ArrayBufferWriter(maxIndex);
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

        return buffer.ReadableSpan.ToArray();
    }
}
