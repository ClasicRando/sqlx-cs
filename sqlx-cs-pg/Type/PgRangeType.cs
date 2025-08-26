using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public abstract class PgRangeType<TValue, TType> : IPgDbType<PgRange<TValue>>
    where TValue : notnull
    where TType : IPgDbType<TValue>
{
    public static void Encode(PgRange<TValue> value, WriteBuffer buffer)
    {
        var flags = RangeFlag.Zero;

        flags |= value.Lower.Type switch
        {
            BoundType.Included => RangeFlag.LowerBoundInclusive,
            BoundType.Unbounded => RangeFlag.LowerBoundInfinite,
            _ => RangeFlag.Zero,
        };
        flags |= value.Upper.Type switch
        {
            BoundType.Included => RangeFlag.UpperBoundInclusive,
            BoundType.Unbounded => RangeFlag.UpperBoundInfinite,
            _ => RangeFlag.Zero,
        };
        buffer.WriteByte((byte)flags);

        switch (value.Lower.Type)
        {
            case BoundType.Included when value.Lower.Value is not null:
            case BoundType.Excluded when value.Lower.Value is not null:
                buffer.WriteLengthPrefixed(false, buf => TType.Encode(value.Lower.Value, buf));
                break;
            case BoundType.Unbounded:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), "Invalid bound type or value");
        }

        switch (value.Upper.Type)
        {
            case BoundType.Included when value.Upper.Value is not null:
            case BoundType.Excluded when value.Upper.Value is not null:
                buffer.WriteLengthPrefixed(false, buf => TType.Encode(value.Upper.Value, buf));
                break;
            case BoundType.Unbounded:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), "Invalid bound type or value");
        }
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public static PgRange<TValue> DecodeBytes(PgBinaryValue value)
    {
        var start = Bound<TValue>.Unbounded();
        var end = Bound<TValue>.Unbounded();

        var flags = (RangeFlag)value.Buffer.ReadByte();
        if (flags.HasFlag(RangeFlag.EmptyRange))
        {
            return new PgRange<TValue>(start, end);
        }

        if (!flags.HasFlag(RangeFlag.LowerBoundInfinite))
        {
            var lowerBoundLength = value.Buffer.ReadInt();
            var lowerBoundValue = new PgBinaryValue(
                value.Buffer.Slice(lowerBoundLength),
                PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary));
            TValue lowerValue = TType.DecodeBytes(lowerBoundValue);
            start = flags.HasFlag(RangeFlag.LowerBoundInclusive)
                ? Bound<TValue>.Included(lowerValue)
                : Bound<TValue>.Excluded(lowerValue);
        }
            
        if (!flags.HasFlag(RangeFlag.UpperBoundInfinite))
        {
            var upperBoundLength = value.Buffer.ReadInt();
            var upperBoundValue = new PgBinaryValue(
                value.Buffer.Slice(upperBoundLength),
                PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary));
            TValue upperValue = TType.DecodeBytes(upperBoundValue);
            end = flags.HasFlag(RangeFlag.UpperBoundInclusive)
                ? Bound<TValue>.Included(upperValue)
                : Bound<TValue>.Excluded(upperValue);
        }

        return new PgRange<TValue>(start, end);
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public static PgRange<TValue> DecodeText(PgTextValue value)
    {
        var start = Bound<TValue>.Unbounded();
        var end = Bound<TValue>.Unbounded();
        
        PgTextValue validChars = value.Slice(1..^1);
        var separatorIndex = FindRangeSeparatorIndex(validChars);
        if (separatorIndex != 0)
        {
            PgTextValue startSlice = validChars.Slice(..(separatorIndex - 1));
            TValue lowerBoundValue = TType.DecodeText(startSlice);
            start = DecodeBound(value.Chars[0], lowerBoundValue, value.ColumnMetadata);
        }

        if (separatorIndex != validChars.Chars.Length)
        {
            PgTextValue endSlice = validChars.Slice((separatorIndex + 1)..);
            TValue upperBoundValue = TType.DecodeText(endSlice);
            end = DecodeBound(value.Chars[0], upperBoundValue, value.ColumnMetadata);
        }
        
        return new PgRange<TValue>(start, end);
    }
    
    public static PgType DbType => PgType.Point;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgRange<TValue> value)
    {
        return DbType;
    }

    private static int FindRangeSeparatorIndex(ReadOnlySpan<char> chars)
    {
        var inQuotes = false;
        var inEscape = false;
        var i = 0;
        for (; i < chars.Length; i++)
        {
            var currentChar = chars[i];
            if (inEscape)
            {
                inEscape = false;
                continue;
            }
            
            switch (currentChar)
            {
                case '"' when inQuotes:
                    inQuotes = false;
                    break;
                case '"':
                    inQuotes = true;
                    break;
                case '\\' when !inEscape:
                    inEscape = true;
                    break;
                case ',' when !inQuotes:
                    return i;
            }
        }
        return i;
    }

    private static Bound<TValue> DecodeBound(
        char chr,
        TValue value,
        PgColumnMetadata columnMetadata)
    {
        return chr switch
        {
            '(' or ')' => Bound<TValue>.Excluded(value),
            '[' or ']' => Bound<TValue>.Included(value),
            _ => throw ColumnDecodeError.Create<PgRange<TValue>>(
                columnMetadata,
                $"Illegal bound character '{chr}'"),
        };
    }
}

[Flags]
public enum RangeFlag : byte
{
    Zero = 0x00,
    EmptyRange = 0x01,
    LowerBoundInclusive = 0x02,
    UpperBoundInclusive = 0x04,
    LowerBoundInfinite = 0x08,
    UpperBoundInfinite = 0x10,
}
