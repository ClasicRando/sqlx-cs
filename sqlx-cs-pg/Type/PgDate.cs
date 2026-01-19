using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="DateOnly"/> values. Maps to the <c>DATE</c> type.
/// </summary>
internal abstract class PgDate : IPgDbType<DateOnly>, IHasRangeType, IHasArrayType
{
    private static readonly DateOnly PostgresEpoch = new(2000, 1, 1);
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the number of days from the <see cref="PostgresEpoch"/> as an <see cref="int"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L209">pg source code</a>
    /// </summary>
    public static void Encode(DateOnly value, IBufferWriter<byte> buffer)
    {
        buffer.WriteInt(value.DayNumber - PostgresEpoch.DayNumber);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Read an <see cref="int"/> as the days since <see cref="PostgresEpoch"/> and add that to the
    /// <see cref="PostgresEpoch"/> to get the date.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L231">pg source code</a>
    /// </summary>
    public static DateOnly DecodeBytes(ref PgBinaryValue value)
    {
        return PostgresEpoch.AddDays(value.Buffer.ReadInt());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Read the characters as an ISO local date
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L184">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If a date cannot be parsed from the characters
    /// </exception>
    public static DateOnly DecodeText(in PgTextValue value)
    {
        if (DateOnly.TryParseExact(value.Chars, "yyyy-MM-dd", out DateOnly date))
        {
            return date;
        }
        
        throw ColumnDecodeException.Create<DateOnly>(
            value.ColumnMetadata,
            $"Cannot parse '{value.Chars}' as a DateOnly");
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Date;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.DateArray;

    public static PgTypeInfo RangeType => PgTypeInfo.Daterange;

    public static PgTypeInfo RangeArrayType => PgTypeInfo.DaterangeArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }
}
