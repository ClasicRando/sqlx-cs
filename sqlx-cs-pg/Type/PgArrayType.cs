using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public abstract class PgArrayType<TElement, TType> : IPgDbType<TElement?[]>
    where TType : IPgDbType<TElement>
    where TElement : notnull
{
    public static void Encode(TElement?[] value, WriteBuffer buffer)
    {
        buffer.WriteInt(1);
        buffer.WriteInt(0);
        buffer.WriteInt(TType.DbType.TypeOid);
        buffer.WriteInt(value.Length);
        buffer.WriteInt(1);
        foreach (TElement? element in value)
        {
            if (element is null)
            {
                buffer.WriteInt(-1);
                continue;
            }
            buffer.WriteLengthPrefixed(false, buf => TType.Encode(element, buf));
        }
    }

    public static TElement?[] DecodeBytes(PgBinaryValue value)
    {
        var dimensions = value.Buffer.ReadInt();
        if (dimensions == 0)
        {
            return [];
        }
        ColumnDecodeError.CheckOrThrow<TElement[]>(
            dimensions == 1,
            value.ColumnMetadata,
            $"Attempted to decode an array of {dimensions} dimensions. Only 1-dimensional arrays are supported");

        // Discard flags value. No longer in use
        value.Buffer.ReadInt();

        var elementTypeOid = value.Buffer.ReadInt();
        ColumnDecodeError.CheckOrThrow<TElement[]>(
            elementTypeOid == TType.DbType.TypeOid,
            value.ColumnMetadata,
            $"Attempted to read an array with another element type. Expected {TType.DbType.TypeOid} but found {elementTypeOid}");
        
        var length = value.Buffer.ReadInt();
        var lowerBound = value.Buffer.ReadInt();
        
        ColumnDecodeError.CheckOrThrow<TElement[]>(
            lowerBound == 1,
            value.ColumnMetadata,
            $"Attempted to read an array with a lower bound other than 1. Got {lowerBound}");

        var result = new TElement?[length];
        for (var i = 0; i < length; i++)
        {
            var elementLength = value.Buffer.ReadInt();
            if (elementLength == -1)
            {
                result[i] = default;
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
        List<TElement?> result = [];
        var valueSlice = value.Chars[1..^1];
        if (valueSlice.IsEmpty)
        {
            return [];
        }

        var isDone = false;
        var startIndex = 0;

        while (!isDone)
        {
            var foundDelimiter = false;
            var inQuotes = false;

            var i = startIndex;
            for (; i < valueSlice.Length; i++)
            {
                var currentChar = valueSlice[i];
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
            var slice = valueSlice[startIndex..i];
            if (slice.IsEmpty || slice is "NULL")
            {
                result.Add(default);
            }
            else
            {
                var elementValue = new PgTextValue(
                    slice,
                    PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Text));
                result.Add(TType.DecodeText(elementValue));
            }

            startIndex = i + 1;
        }

        return result.ToArray();
    }
    
    public static PgType DbType => TType.ArrayDbType;

    public static PgType ArrayDbType => DbType;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(TElement?[] value)
    {
        return DbType;
    }
}
