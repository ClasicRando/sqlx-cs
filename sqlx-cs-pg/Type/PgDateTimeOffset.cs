using System.Buffers;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="DateTimeOffset"/> values. Maps to the
/// <c>TIMESTAMP WITH TIME ZONE</c> type but is compatible with <c>TIMESTAMP WITHOUT TIME ZONE</c>.
/// </summary>
public abstract class PgDateTimeOffset : IPgDbType<DateTimeOffset>, IHasRangeType, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes <see cref="DateTimeOffset.UtcDateTime"/> using <see cref="PgDateTime.Encode"/>
    /// </summary>
    public static void Encode(DateTimeOffset value, IBufferWriter<byte> buffer)
    {
        PgDateTime.Encode(value.UtcDateTime, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Reads the bytes as a <see cref="PgDateTime"/> using <see cref="PgDateTime.DecodeBytes"/> and
    /// creates a new <see cref="DateTimeOffset"/> using an offset of <see cref="TimeSpan.Zero"/>.
    /// This is because the client's Timezone parameter is assumed to be UTC.
    /// </summary>
    public static DateTimeOffset DecodeBytes(in PgBinaryValue value)
    {
        return new DateTimeOffset(PgDateTime.DecodeBytes(value), TimeSpan.Zero);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Reads the characters as a <see cref="PgDateTime"/> using <see cref="PgDateTime.DecodeText"/>
    /// and creates a new <see cref="DateTimeOffset"/> using an offset of
    /// <see cref="TimeSpan.Zero"/>. This is because the client's Timezone parameter is assumed to
    /// be UTC.
    /// </summary>
    public static DateTimeOffset DecodeText(in PgTextValue value)
    {
        return new DateTimeOffset(PgDateTime.DecodeText(value), TimeSpan.Zero);
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Timestamptz;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.TimestamptzArray;

    public static PgTypeInfo RangeType => PgTypeInfo.Tstzrange;

    public static PgTypeInfo RangeArrayType => PgTypeInfo.TstzrangeArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return PgDateTime.IsCompatible(typeInfo);
    }
}
