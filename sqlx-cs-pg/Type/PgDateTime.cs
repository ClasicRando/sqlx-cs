using System.Buffers;
using System.Globalization;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="DateTime"/> values. Maps to the
/// <c>TIMESTAMP WITHOUT TIME ZONE</c> type but is compatible with <c>TIMESTAMP WITH TIME ZONE</c>.
/// </summary>
public abstract class PgDateTime : IPgDbType<DateTime>, IHasRangeType, IHasArrayType
{
    private const long PostgresEpochSeconds = 946_684_800;
    private const long PostgresEpochTicks = PostgresEpochSeconds * TimeSpan.TicksPerSecond;
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the number of microseconds since the postgres epoch ("2000-01-01 00:00:00") as a
    /// <see cref="long"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/timestamp.c#L259">pg source code</a>
    /// </summary>
    public static void Encode(DateTime value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.WriteLong((value.Ticks - PostgresEpochTicks) / TimeSpan.TicksPerMicrosecond);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Read a <see cref="long"/> from the buffer and use that as the number of microseconds since
    /// the postgres epoch ("2000-01-01 00:00:00")
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/timestamp.c#L292">pg source code</a>
    /// </summary>
    public static DateTime DecodeBytes(ref PgBinaryValue value)
    {
        return new DateTime(value.Buffer.ReadLong() * TimeSpan.TicksPerMicrosecond + PostgresEpochTicks);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Read the characters as an ISO local date time
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/timestamp.c#L233">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters cannot be parsed into a <see cref="DateTime"/>
    /// </exception>
    public static DateTime DecodeText(in PgTextValue value)
    {
        if (DateTime.TryParse(value.Chars, null, DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
        {
            return dateTime;
        }
        
        throw ColumnDecodeException.Create<DateTime>(
            value.ColumnMetadata,
            $"Cannot parse '{value.Chars}' as a DateTime");
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Timestamp;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.TimestampArray;

    public static PgTypeInfo RangeType => PgTypeInfo.Tsrange;

    public static PgTypeInfo RangeArrayType => PgTypeInfo.TsrangeArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType || typeInfo == PgTypeInfo.Timestamptz;
    }
}
