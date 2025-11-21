using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="TimeOnly"/> values. Maps to the <c>TIME</c> type.
/// </summary>
internal abstract class PgTime : IPgDbType<TimeOnly>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the number of microseconds since the start of the day.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L1521">pg source code</a>
    /// </summary>
    public static void Encode(TimeOnly value, WriteBuffer buffer)
    {
        
        buffer.WriteLong(value.Ticks / TimeSpan.TicksPerMicrosecond);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Read a <see cref="long"/> value as the microseconds from the start of the date and use that
    /// to construct a <see cref="TimeOnly"/> by converting that value to ticks and passing it to
    /// <see cref="TimeOnly(long)"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L1547">pg source code</a>
    /// </summary>
    public static TimeOnly DecodeBytes(ref PgBinaryValue value)
    {
        var microSeconds = value.Buffer.ReadLong();
        return new TimeOnly(microSeconds * TimeSpan.TicksPerMicrosecond);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Parse the characters as a <see cref="TimeOnly"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/date.c#L1501">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters cannot be parsed as a <see cref="TimeOnly"/>
    /// </exception>
    public static TimeOnly DecodeText(PgTextValue value)
    {
        if (!TimeOnly.TryParse(value, out TimeOnly time))
        {
            throw ColumnDecodeException.Create<TimeOnly>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a time value");
        }
        
        return time;
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Time;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.TimeArray;

    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return dbType == DbType;
    }
}
