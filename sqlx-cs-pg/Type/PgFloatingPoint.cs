using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Helper class for reading floating point values
/// </summary>
internal static class PgFloatingPoint
{
    public static double ExtractFloat<T>(this PgBinaryValue value) where T : notnull
    {
        return value.Buffer.Remaining switch
        {
            4 => value.Buffer.ReadFloat(),
            8 => value.Buffer.ReadDouble(),
            _ => throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Could not extract float from buffer. Number of bytes = {value.Buffer.Remaining}"),
        };
    }
    
    public static double ExtractFloat<T>(this PgTextValue value) where T : notnull
    {
        if (!double.TryParse(value, null, out var parseResult))
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Could not convert {value} into {typeof(T)}");
        }
        return parseResult;
    }
    
    public static bool IsFloatCompatible(PgType dbType)
    {
        return dbType.TypeOid == PgType.Float4.TypeOid
               || dbType.TypeOid == PgType.Float8.TypeOid;
    }
}

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="double"/> values. Maps to the <c>DOUBLE PRECISION</c>
/// type.
/// </summary>
internal abstract class PgDouble : IPgDbType<double>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the <see cref="double"/> value to the buffer
    /// </summary>
    public static void Encode(double value, WriteBuffer buffer)
    {
        buffer.WriteDouble(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read the bytes available to get a floating point number. If the underlining value is a
    /// <see cref="float"/> it's casted to a <see cref="double"/>.
    /// </summary>
    public static double DecodeBytes(PgBinaryValue value)
    {
        return value.ExtractFloat<double>();
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Attempts to parse the characters as a <see cref="double"/>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not a <see cref="double"/>
    /// </exception>
    public static double DecodeText(PgTextValue value)
    {
        return value.ExtractFloat<double>();
    }

    public static PgType DbType => PgType.Float8;

    public static PgType ArrayDbType => PgType.Float8Array;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgFloatingPoint.IsFloatCompatible(dbType);
    }

    public static PgType GetActualType(double value)
    {
        return DbType;
    }
}

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="float"/> values. Maps to the <c>REAL</c> type.
/// </summary>
internal abstract class PgFloat : IPgDbType<float>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the <see cref="float"/> value to the buffer
    /// </summary>
    public static void Encode(float value, WriteBuffer buffer)
    {
        buffer.WriteFloat(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read the bytes available to get a floating point number. Fails if the underlining value is
    /// outside the valid range for a <see cref="float"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the <see cref="double"/> value extracted is not a valid float
    /// </exception>
    public static float DecodeBytes(PgBinaryValue value)
    {
        return ValidateFloat(value.ExtractFloat<float>(), value.ColumnMetadata);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Attempts to parse the characters as a <see cref="double"/> and cast to a <see cref="float"/>
    /// if the value is within the valid range of floats.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not a <see cref="double"/> or the <see cref="double"/> value is not a
    /// valid float
    /// </exception>
    public static float DecodeText(PgTextValue value)
    {
        return ValidateFloat(value.ExtractFloat<float>(), value.ColumnMetadata);
    }

    private static float ValidateFloat(double floatingPoint, PgColumnMetadata columnMetadata)
    {
        if (floatingPoint is < float.MinValue or > float.MaxValue)
        {
            throw ColumnDecodeException.Create<float>(
                columnMetadata,
                "Floating point value is outside the bounds of float");
        }
        return (float)floatingPoint;
    }

    public static PgType DbType => PgType.Float4;

    public static PgType ArrayDbType => PgType.Float4Array;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgFloatingPoint.IsFloatCompatible(dbType);
    }

    public static PgType GetActualType(float value)
    {
        return DbType;
    }
}
