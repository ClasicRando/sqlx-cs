using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// <see cref="IPgDbType{T}"/> for <see cref="sbyte"/> values. Maps to the <c>"CHAR"</c> type.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-character.html#DATATYPE-CHARACTER-SPECIAL-TABLE">docs</a>
/// </summary>
internal abstract class PgChar : IPgDbType<sbyte>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Simply writes the <see cref="sbyte"/> value to the buffer as a <see cref="byte"/>
    /// </summary>
    public static void Encode(sbyte value, WriteBuffer buffer)
    {
        buffer.WriteByte((byte)value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Reads the first byte from the value buffer provided. If not bytes are remaining, returns 0
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/char.c#L105">pg source code</a>
    /// </summary>
    public static sbyte DecodeBytes(ref PgBinaryValue value)
    {
        return value.Buffer.Remaining == 0 ? (sbyte)0 : (sbyte)value.Buffer.ReadByte();
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Converts the text into a <see cref="sbyte"/> depending on the number of characters:
    /// <list type="bullet">
    ///     <item>
    ///     when 4, the <see cref="sbyte"/> has been packed into 3 characters with a forward slash
    ///     prefix to accommodate non-ascii char values, see pg code for more details explanation
    ///     </item>
    ///     <item>
    ///     when 1, the <see cref="sbyte"/> is just the ascii representation of the character
    ///     </item>
    ///     <item>when 0, return 0</item>
    /// </list>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/char.c#L64">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the text representation is not the expected length
    /// </exception>
    public static sbyte DecodeText(PgTextValue value)
    {
        return value.Chars.Length switch
        {
            4 => (sbyte)(((value.Chars[1] - '0') << 6) | ((value.Chars[2] - '0') << 3) | (value.Chars[3] - '0')),
            1 => (sbyte)value.Chars[0],
            0 => 0,
            _ => throw ColumnDecodeException.Create<sbyte>(
                value.ColumnMetadata,
                $"Received invalid \"char\" text, {value}"),
        };
    }

    public static PgTypeInfo DbType => PgTypeInfo.Char;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.CharArray;

    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return dbType == DbType;
    }
}
