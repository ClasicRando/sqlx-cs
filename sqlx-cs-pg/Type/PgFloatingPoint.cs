using System.Buffers;
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
    public static double ExtractFloat<T>(in this PgBinaryValue value) where T : notnull
    {
        var buff = value.Buffer;
        return buff.Length switch
        {
            4 => buff.ReadFloat(),
            8 => buff.ReadDouble(),
            _ => throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Could not extract float from buffer. Number of bytes = {value.Buffer.Length}"),
        };
    }

    public static double ExtractFloat<T>(in this PgTextValue value) where T : notnull
    {
        if (!double.TryParse(value.Chars, null, out var parseResult))
        {
            throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Could not convert '{value.Chars}' into {typeof(T)}");
        }

        return parseResult;
    }

    public static bool IsFloatCompatible(PgTypeInfo dbType)
    {
        return dbType == PgTypeInfo.Float4 || dbType == PgTypeInfo.Float8;
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
    public static void Encode(double value, IBufferWriter<byte> buffer)
    {
        buffer.WriteDouble(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read the bytes available to get a floating point number. If the underlining value is a
    /// <see cref="float"/> it's cast to a <see cref="double"/>.
    /// </summary>
    public static double DecodeBytes(in PgBinaryValue value)
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
    public static double DecodeText(in PgTextValue value)
    {
        return value.ExtractFloat<double>();
    }

    public static PgTypeInfo DbType => PgTypeInfo.Float8;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.Float8Array;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return PgFloatingPoint.IsFloatCompatible(typeInfo);
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
    public static void Encode(float value, IBufferWriter<byte> buffer)
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
    public static float DecodeBytes(in PgBinaryValue value)
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
    public static float DecodeText(in PgTextValue value)
    {
        return ValidateFloat(value.ExtractFloat<float>(), value.ColumnMetadata);
    }

    private static float ValidateFloat(double floatingPoint, PgColumnMetadata columnMetadata)
    {
        if (floatingPoint is < float.MinValue or > float.MaxValue)
        {
            throw ColumnDecodeException.Create<float, PgColumnMetadata>(
                columnMetadata,
                "Floating point value is outside the bounds of float");
        }

        return (float)floatingPoint;
    }

    public static PgTypeInfo DbType => PgTypeInfo.Float4;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.Float4Array;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return PgFloatingPoint.IsFloatCompatible(typeInfo);
    }
}
