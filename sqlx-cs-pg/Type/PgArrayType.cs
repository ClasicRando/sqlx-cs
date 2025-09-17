using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal static class PgArrayTypeUtils
{
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

public abstract class PgArrayTypeClass<TElement, TType> : IPgDbType<TElement?[]>
    where TType : IPgDbType<TElement>, IHasArrayType
    where TElement : class
{
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
            
            var binaryValue = new PgBinaryValue(
                value.Buffer.Slice(elementLength),
                PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary));
            result[i] = TType.DecodeBytes(binaryValue);
        }

        return result;
    }

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
            var elementValue = new PgTextValue(
                slice,
                PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Text));
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

public abstract class PgArrayTypeStruct<TElement, TType> : IPgDbType<TElement?[]>
    where TType : IPgDbType<TElement>, IHasArrayType
    where TElement : struct
{
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
            
            var binaryValue = new PgBinaryValue(
                value.Buffer.Slice(elementLength),
                PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary));
            result[i] = TType.DecodeBytes(binaryValue);
        }

        return result;
    }

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
            var elementValue = new PgTextValue(
                slice,
                PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Text));
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
