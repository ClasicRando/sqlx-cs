using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Types;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Helper class for reading integer values
/// </summary>
internal static class PgInteger
{
    /// <summary>
    /// Extract a <see cref="long"/> value from the buffer, up casting values to a long if the
    /// number of remaining bytes does not equal 8
    /// </summary>
    /// <param name="value">Binary encoded value</param>
    /// <typeparam name="T">Integer type</typeparam>
    /// <returns><see cref="long"/> value from the buffer</returns>
    /// <exception cref="ColumnDecodeException">
    /// If the number of bytes is not 2, 4 or 8
    /// </exception>
    public static long ExtractInteger<T>(in this PgBinaryValue value) where T : notnull
    {
        var buff = value.Buffer;
        return buff.Length switch
        {
            2 => buff.ReadShort(),
            4 when typeof(T) == typeof(uint) => buff.ReadUInt(),
            4 => buff.ReadInt(),
            8 => buff.ReadLong(),
            _ => throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Could not extract integer from buffer. Number of bytes = {value.Buffer.Length}"),
        };
    }

    /// <summary>
    /// Extract a <see cref="long"/> value from the characters
    /// </summary>
    /// <param name="value">Text encode value</param>
    /// <typeparam name="T">Integer type</typeparam>
    /// <returns><see cref="long"/> value from the characters</returns>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not a valid <see cref="long"/> value
    /// </exception>
    public static long ExtractInteger<T>(in this PgTextValue value) where T : notnull
    {
        if (!long.TryParse(value.Chars, null, out var parseResult))
        {
            throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Could not convert '{value.Chars}' into {typeof(T)}");
        }

        return parseResult;
    }

    public static bool IsIntegerCompatible(PgTypeInfo dbType)
    {
        return dbType == PgTypeInfo.Int8
               || dbType == PgTypeInfo.Int4
               || dbType == PgTypeInfo.Int2
               || dbType == PgTypeInfo.Oid;
    }
    
    /// <summary>
    /// Check to see if this <see cref="long"/> value is a valid <see cref="uint"/>
    /// </summary>
    /// <param name="value">long value to check</param>
    /// <param name="columnMetadata">column metadata to construct the exception</param>
    /// <returns>the value cast to an uint</returns>
    /// <exception cref="ColumnDecodeException">if the value is not a valid uint</exception>
    public static uint ValidateUInt(long value, in PgColumnMetadata columnMetadata)
    {
        if (!Integers.IsValidUInt(value))
        {
            Integers.ThrowColumnDecodeException<uint, PgColumnMetadata>(columnMetadata);
        }
        return (uint)value;
    }
    
    /// <summary>
    /// Check to see if this <see cref="long"/> value is a valid int
    /// </summary>
    /// <param name="value">long value to check</param>
    /// <param name="columnMetadata">column metadata to construct the exception</param>
    /// <returns>the value cast to an int</returns>
    /// <exception cref="ColumnDecodeException">if the value is not a valid int</exception>
    public static int ValidateInt(long value, in PgColumnMetadata columnMetadata)
    {
        if (!Integers.IsValidInt(value))
        {
            Integers.ThrowColumnDecodeException<int, PgColumnMetadata>(columnMetadata);
        }
        return (int)value;
    }
    
    /// <summary>
    /// Check to see if this <see cref="long"/> value is a valid short
    /// </summary>
    /// <param name="value">long value to check</param>
    /// <param name="columnMetadata">column metadata to construct the exception</param>
    /// <returns>the value cast to a short</returns>
    /// <exception cref="ColumnDecodeException">if the value is not a valid short</exception>
    public static short ValidateShort(long value, in PgColumnMetadata columnMetadata)
    {
        if (!Integers.IsValidShort(value))
        {
            Integers.ThrowColumnDecodeException<short, PgColumnMetadata>(columnMetadata);
        }
        return (short)value;
    }
}

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="long"/> values. Maps to the <c>BIGINT</c> type.
/// </summary>
internal abstract class PgLong : IPgDbType<long>, IHasRangeType, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the <see cref="long"/> value to the buffer
    /// </summary>
    public static void Encode(long value, IBufferWriter<byte> buffer)
    {
        buffer.WriteLong(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read a <see cref="long"/> value from the buffer
    /// </summary>
    public static long DecodeBytes(in PgBinaryValue value)
    {
        return value.ExtractInteger<long>();
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Parse the characters to a <see cref="long"/> value
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not a <see cref="long"/> value
    /// </exception>
    public static long DecodeText(in PgTextValue value)
    {
        return value.ExtractInteger<long>();
    }

    public static PgTypeInfo DbType => PgTypeInfo.Int8;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.Int8Array;

    public static PgTypeInfo RangeType => PgTypeInfo.Int8Range;

    public static PgTypeInfo RangeArrayType => PgTypeInfo.Int8RangeArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return PgInteger.IsIntegerCompatible(typeInfo);
    }
}

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="int"/> values. Maps to the <c>INTEGER</c> type.
/// </summary>
internal abstract class PgInt : IPgDbType<int>, IHasRangeType, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the <see cref="int"/> value to the buffer
    /// </summary>
    public static void Encode(int value, IBufferWriter<byte> buffer)
    {
        buffer.WriteInt(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read an <see cref="int"/> value from the buffer. Down casts if the actual value is a
    /// <see cref="long"/> but can be safely fit within an <see cref="int"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the integer value is outside a valid <see cref="int"/>
    /// </exception>
    public static int DecodeBytes(in PgBinaryValue value)
    {
        var integer = value.ExtractInteger<int>();
        return PgInteger.ValidateInt(integer, value.ColumnMetadata);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Parse the characters to an <see cref="int"/> value. Down casts if the actual value is a
    /// <see cref="long"/> but can be safely fit within an <see cref="int"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not an <see cref="int"/> value
    /// </exception>
    public static int DecodeText(in PgTextValue value)
    {
        var integer = value.ExtractInteger<int>();
        return PgInteger.ValidateInt(integer, value.ColumnMetadata);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Int4;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.Int4Array;

    public static PgTypeInfo RangeType => PgTypeInfo.Int4Range;

    public static PgTypeInfo RangeArrayType => PgTypeInfo.Int4RangeArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return PgInteger.IsIntegerCompatible(typeInfo);
    }
}

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="short"/> values. Maps to the <c>SMALLINT</c> type.
/// </summary>
internal abstract class PgShort : IPgDbType<short>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the <see cref="short"/> value to the buffer
    /// </summary>
    public static void Encode(short value, IBufferWriter<byte> buffer)
    {
        buffer.WriteShort(value);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read a <see cref="short"/> value from the buffer. Down casts if the actual value is a
    /// <see cref="long"/> or <see cref="int"/> but can be safely fit within a <see cref="short"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the integer value is outside a valid <see cref="short"/>
    /// </exception>
    public static short DecodeBytes(in PgBinaryValue value)
    {
        var integer = value.ExtractInteger<short>();
        return PgInteger.ValidateShort(integer, value.ColumnMetadata);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Parse the characters to a <see cref="short"/> value. Down casts if the actual value is a
    /// <see cref="long"/> or <see cref="int"/> but can be safely fit within a <see cref="short"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not a <see cref="short"/> value
    /// </exception>
    public static short DecodeText(in PgTextValue value)
    {
        var integer = value.ExtractInteger<short>();
        return PgInteger.ValidateShort(integer, value.ColumnMetadata);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Int2;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.Int2Array;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return PgInteger.IsIntegerCompatible(typeInfo);
    }
}
