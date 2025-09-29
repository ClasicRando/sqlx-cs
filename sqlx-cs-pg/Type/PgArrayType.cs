using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// Helper class for Postgres array type encoding and decoding
/// </summary>
internal static class PgArrayTypeUtils
{
    /// <summary>
    /// Encode initial metadata fields for the binary encoded array value. This writes
    /// <list type="number">
    ///     <item>The number of dimensions (always 1)</item>
    ///     <item>Array header flags (not used so always 0)</item>
    ///     <item>The OID of the element type</item>
    ///     <item>The number of items in the array</item>
    ///     <item>The lower bound of the array (always 1)</item>
    /// </list>
    /// </summary>
    /// <param name="length">Number of elements in the array</param>
    /// <param name="buffer">Buffer to write the metadata to</param>
    /// <typeparam name="TElement">Array element type</typeparam>
    /// <typeparam name="TType">Array element's <see cref="IPgDbType{T}"/></typeparam>
    public static void EncodeMetaFields<TElement, TType>(int length, WriteBuffer buffer)
        where TType : IPgDbType<TElement>, IHasArrayType
        where TElement : notnull
    {
        buffer.WriteInt(1);
        buffer.WriteInt(0);
        buffer.WriteInt(TType.DbType.TypeOid);
        buffer.WriteInt(length);
        buffer.WriteInt(1);
    }

    /// <summary>
    /// Decode the initial metadata fields from the value buffer
    /// </summary>
    /// <param name="value">Binary encoded value</param>
    /// <typeparam name="TElement">Array element type</typeparam>
    /// <typeparam name="TType">Array element's <see cref="IPgDbType{T}"/></typeparam>
    /// <returns>Number of items in the array</returns>
    public static int DecodeMetaFields<TElement, TType>(PgBinaryValue value)
        where TType : IPgDbType<TElement>
        where TElement : notnull
    {
        var dimensions = value.Buffer.ReadInt();
        if (dimensions == 0)
        {
            return 0;
        }
        ColumnDecodeException.CheckOrThrow<TElement[]>(
            dimensions == 1,
            value.ColumnMetadata,
            $"Attempted to decode an array of {dimensions} dimensions. Only 1-dimensional arrays are supported");

        // Discard flags value. No longer in use
        value.Buffer.ReadInt();

        var elementTypeOid = value.Buffer.ReadInt();
        ColumnDecodeException.CheckOrThrow<TElement[]>(
            elementTypeOid == TType.DbType.TypeOid,
            value.ColumnMetadata,
            $"Attempted to read an array with another element type. Expected {TType.DbType.TypeOid} but found {elementTypeOid}");
        
        var length = value.Buffer.ReadInt();
        var lowerBound = value.Buffer.ReadInt();
        
        ColumnDecodeException.CheckOrThrow<TElement[]>(
            lowerBound == 1,
            value.ColumnMetadata,
            $"Attempted to read an array with a lower bound other than 1. Got {lowerBound}");
        
        return length;
    }

    /// <summary>
    /// Gather the ranges of characters inside the character buffer that represent each element of
    /// the array. 
    /// </summary>
    /// <param name="value">Text encoded value</param>
    /// <returns>List of ranges for each array element in buffer</returns>
    public static List<Range?> DecodeTextRanges(PgTextValue value)
    {
        if (value.Chars is "{}")
        {
            return [];
        }
        
        List<Range?> result = [];
        var lastIndex = value.Chars.Length - 2;
        var isDone = false;
        var startIndex = 1;

        while (!isDone)
        {
            var foundDelimiter = false;
            var inQuotes = false;

            var i = startIndex;
            for (; i <= lastIndex; i++)
            {
                var currentChar = value.Chars[i];
                switch (currentChar)
                {
                    case '"':
                        inQuotes = !inQuotes;
                        break;
                    case '\\':
                        i++;
                        break;
                    case ',' when !inQuotes:
                        foundDelimiter = true;
                        break;
                }
                
                if (foundDelimiter) break;
            }

            isDone = !foundDelimiter;
            var slice = value.Chars[startIndex..i];
            if (slice.IsEmpty || slice is "NULL")
            {
                result.Add(null);
            }
            else
            {
                result.Add(startIndex..i);
            }

            startIndex = i + 1;
        }

        return result;
    }
}

/// <summary>
/// Array type decoder for reference types. Functionally similar to
/// <see cref="PgArrayTypeStruct{TElement,TType}"/> but in both cases we need to account for nulls
/// in arrays which is handled differently by the semantics of each type group.
/// </summary>
/// <typeparam name="TElement">Array element type</typeparam>
/// <typeparam name="TType">Array element's <see cref="IPgDbType{T}"/></typeparam>
internal abstract class PgArrayTypeClass<TElement, TType> : IPgDbType<TElement?[]>
    where TType : IPgDbType<TElement>, IHasArrayType
    where TElement : class
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Encode an array of elements into the supplied buffer. This writes the metadata fields using
    /// <see cref="PgArrayTypeUtils.EncodeMetaFields"/> followed by each item encoded into the
    /// buffer (length prefixed if not null).
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/d57b7cc3338e9d9aa1d7c5da1b25a17c5a72dcce/src/backend/utils/adt/arrayfuncs.c#L1272">pg source code</a>
    /// </summary>
    public static void Encode(TElement?[] value, WriteBuffer buffer)
    {
        PgArrayTypeUtils.EncodeMetaFields<TElement, TType>(value.Length, buffer);
        foreach (TElement? element in value)
        {
            if (element is null)
            {
                buffer.WriteInt(-1);
                continue;
            }
            buffer.StartWritingLengthPrefixed();
            TType.Encode(element, buffer);
            buffer.FinishWritingLengthPrefixed(includeLength: false);
        }
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Decode an array of element from the supplied binary encoded buffer. This reads the metadata
    /// fields using <see cref="PgArrayTypeUtils.DecodeMetaFields"/> followed by each item decoded
    /// from the buffer. Items are length prefixed to know how much data an element includes where
    /// a length of -1 means a null value.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/d57b7cc3338e9d9aa1d7c5da1b25a17c5a72dcce/src/backend/utils/adt/arrayfuncs.c#L1549">pg source code</a>
    /// </summary>
    public static TElement?[] DecodeBytes(PgBinaryValue value)
    {
        var length = PgArrayTypeUtils.DecodeMetaFields<TElement, TType>(value);
        if (length == 0)
        {
            return [];
        }

        var result = new TElement?[length];
        for (var i = 0; i < length; i++)
        {
            var elementLength = value.Buffer.ReadInt();
            if (elementLength == -1)
            {
                result[i] = null;
                continue;
            }

            result[i] = TType.DecodeBytes(value.Slice(elementLength));
        }

        return result;
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Message is a text representation of the array (also called an array literal). Must be
    /// wrapped in curly braces and each item is separated by a comma. This utilizes a method
    /// <see cref="PgArrayTypeUtils.DecodeTextRanges"/> that scans the character buffer for each
    /// element's range. With all the ranges found, the characters buffer is sliced for each range
    /// and that slice is passed to the element types decoder. 
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1842">pg source code</a>
    /// </summary>
    public static TElement?[] DecodeText(PgTextValue value)
    {
        var ranges = PgArrayTypeUtils.DecodeTextRanges(value);
        var result = new TElement?[ranges.Count];
        
        for (var index = 0; index < ranges.Count; index++)
        {
            var rng = ranges[index];
            if (!rng.HasValue)
            {
                result[index] = null;
                continue;
            }

            result[index] = TType.DecodeText(value.Slice(rng.Value));
        }

        return result;
    }
    
    public static PgType DbType => TType.ArrayDbType;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(TElement?[] value)
    {
        return DbType;
    }
}

/// <summary>
/// Array type decoder for value types. Functionally similar to
/// <see cref="PgArrayTypeClass{TElement,TType}"/> but in both cases we need to account for nulls
/// in arrays which is handled differently by the semantics of each type group. In this class, we
/// can not use <c>default</c> since that means an empty struct, not a database null which should be
/// the actual value.
/// </summary>
/// <typeparam name="TElement">Array element type</typeparam>
/// <typeparam name="TType">Array element's <see cref="IPgDbType{T}"/></typeparam>
internal abstract class PgArrayTypeStruct<TElement, TType> : IPgDbType<TElement?[]>
    where TType : IPgDbType<TElement>, IHasArrayType
    where TElement : struct
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Encode an array of elements into the supplied buffer. This writes the metadata fields using
    /// <see cref="PgArrayTypeUtils.EncodeMetaFields"/> followed by each item encoded into the
    /// buffer (length prefixed if not null).
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/d57b7cc3338e9d9aa1d7c5da1b25a17c5a72dcce/src/backend/utils/adt/arrayfuncs.c#L1272">pg source code</a>
    /// </summary>
    public static void Encode(TElement?[] value, WriteBuffer buffer)
    {
        PgArrayTypeUtils.EncodeMetaFields<TElement, TType>(value.Length, buffer);
        foreach (var element in value)
        {
            if (!element.HasValue)
            {
                buffer.WriteInt(-1);
                continue;
            }
            buffer.StartWritingLengthPrefixed();
            TType.Encode(element.Value, buffer);
            buffer.FinishWritingLengthPrefixed(includeLength: false);
        }
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Decode an array of element from the supplied binary encoded buffer. This reads the metadata
    /// fields using <see cref="PgArrayTypeUtils.DecodeMetaFields"/> followed by each item decoded
    /// from the buffer. Items are length prefixed to know how much data an element includes where
    /// a length of -1 means a null value.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/d57b7cc3338e9d9aa1d7c5da1b25a17c5a72dcce/src/backend/utils/adt/arrayfuncs.c#L1549">pg source code</a>
    /// </summary>
    public static TElement?[] DecodeBytes(PgBinaryValue value)
    {
        var length = PgArrayTypeUtils.DecodeMetaFields<TElement, TType>(value);
        if (length == 0)
        {
            return [];
        }

        var result = new TElement?[length];
        for (var i = 0; i < length; i++)
        {
            var elementLength = value.Buffer.ReadInt();
            if (elementLength == -1)
            {
                result[i] = null;
                continue;
            }

            var columnMetadata = PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary);
            var binaryValue = new PgBinaryValue(
                value.Buffer.Slice(elementLength),
                ref columnMetadata);
            result[i] = TType.DecodeBytes(binaryValue);
        }

        return result;
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Message is a text representation of the array (also called an array literal). Must be
    /// wrapped in curly braces and each item is separated by a comma. This utilizes a method
    /// <see cref="PgArrayTypeUtils.DecodeTextRanges"/> that scans the character buffer for each
    /// element's range. With all the ranges found, the characters buffer is sliced for each range
    /// and that slice is passed to the element types decoder. 
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1842">pg source code</a>
    /// </summary>
    public static TElement?[] DecodeText(PgTextValue value)
    {
        var ranges = PgArrayTypeUtils.DecodeTextRanges(value);
        var result = new TElement?[ranges.Count];
        
        for (var index = 0; index < ranges.Count; index++)
        {
            var rng = ranges[index];
            if (!rng.HasValue)
            {
                result[index] = null;
                continue;
            }

            var slice = value.Chars[rng.Value];
            var columnMetadata = PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Text);
            var elementValue = new PgTextValue(slice, ref columnMetadata);
            result[index] = TType.DecodeText(elementValue);
        }

        return result;
    }
    
    public static PgType DbType => TType.ArrayDbType;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(TElement?[] value)
    {
        return DbType;
    }
}
