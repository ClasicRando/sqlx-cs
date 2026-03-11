using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>TIMETZ</c> type represented as a <see cref="TimeOnly"/> and the offset in seconds
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-datetime.html#DATATYPE-TIMEZONES">docs</a>
/// </summary>
/// <param name="Time">Scalar time in the day</param>
/// <param name="OffsetSeconds">
/// Time zone offset in seconds. Negative offsets indicates timezones west of UTC
/// </param>
public readonly record struct PgTimeTz(TimeOnly Time, int OffsetSeconds)
    : IPgDbType<PgTimeTz>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the number of microseconds since the start of the day followed by the number of
    /// seconds offset from UTC.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L2335">pg source code</a>
    /// </summary>
    public static void Encode(PgTimeTz value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        PgTime.Encode(value.Time, buffer);
        buffer.WriteInt(value.OffsetSeconds);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Extracts a <see cref="TimeOnly"/> using <see cref="PgTime.DecodeBytes"/> then reads an
    /// <see cref="int"/> to get the offset in seconds.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L2371">pg source code</a>
    /// </summary>
    public static PgTimeTz DecodeBytes(in PgBinaryValue value)
    {
        var buff = value.Buffer;
        var timeValue = new PgBinaryValue(buff.ReadBytesAsSpan(PgTime.Size), value.ColumnMetadata);
        TimeOnly time = PgTime.DecodeBytes(timeValue);
        var offsetSeconds = buff.ReadInt();
        return new PgTimeTz(time, offsetSeconds);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Parses the characters up to the offset start index using <see cref="PgTime.DecodeText"/> to
    /// get the time, then parses the offset and return the composite type.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L2314">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the offset cannot be parsed or a time value cannot be extracted
    /// </exception>
    public static PgTimeTz DecodeText(in PgTextValue value)
    {
        var offsetSeconds = FindOffset(value, out var offsetStart);
        TimeOnly time = PgTime.DecodeText(value.Slice(..offsetStart));
        return new PgTimeTz(time, offsetSeconds);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Timetz;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.TimetzArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    private static int FindOffset(in PgTextValue value, out int offsetStart)
    {
        offsetStart = value.Chars.IndexOfAny("Z+-");
        if (offsetStart == -1)
        {
            offsetStart = value.Chars.Length;
            return 0;
        }
        
        var offsetChar = value.Chars[offsetStart];
        if (offsetChar is 'Z')
        {
            return 0;
        }

        var offsetChars = value.Chars[(offsetStart + 1)..];
        Span<Range> splits = stackalloc Range[2];
        var rangeCount = offsetChars.Split(splits, ':');

        var factor = offsetChar == '+' ? 1 : -1;
        var offset = 0;
        var digitMultiplier = 2;
        for (var i = 0; i < rangeCount; i++)
        {
            if (!int.TryParse(offsetChars[splits[i]], out var result))
            {
                throw ColumnDecodeException.Create<PgTimeTz, PgColumnMetadata>(
                    value.ColumnMetadata,
                    $"Could not parse offset from '{value.Chars}'");
            }
            offset += result * (int)Math.Pow(60.0, digitMultiplier--);
        }

        return offset * factor;
    }
}
