using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="PgRange{T}"/> values. Maps to any database type that
/// has a range and the CLR type must implement <see cref="IHasRangeType"/>.
/// </summary>
internal abstract class PgRangeType<TValue, TType> : IPgDbType<PgRange<TValue>>, IHasArrayType
    where TType : IPgDbType<TValue>, IHasRangeType
    where TValue : notnull
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the range flags as a single Byte, followed by the <see cref="PgRange{T}.Lower"/> and
    /// <see cref="PgRange{T}.Upper"/> if either value is not unbounded. Range flags are a bitmap
    /// <see cref="RangeFlag"/> value including bits if the upper/lower bounds are inclusive or
    /// infinite (i.e. unbounded).
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/rangetypes.c#L177">pg source code</a>
    /// </summary>
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
                var startPosition = buffer.StartWritingLengthPrefixed();
                TType.Encode(value.Lower.Value, buffer);
                buffer.FinishWritingLengthPrefixed(startPosition, includeLength: false);
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
                var startPosition = buffer.StartWritingLengthPrefixed();
                TType.Encode(value.Upper.Value, buffer);
                buffer.FinishWritingLengthPrefixed(startPosition, includeLength: false);
                break;
            case BoundType.Unbounded:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), "Invalid bound type or value");
        }
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Steps to decode a generic range:
    /// <list type="number">
    ///     <item>Initialize the bounds as Bound.Unbounded since that is the default value.</item>
    ///     <item>
    ///     Read a Single Byte as the range flags. If the flags value contains the
    ///     <see cref="RangeFlag.EmptyRange"/> then the start and end are unbounded and the decode
    ///     method exits, returning a <see cref="PgRange{T}"/> with those bounds
    ///     </item>
    ///     <item>
    ///     Check if the flags value contains the <see cref="RangeFlag.LowerBoundInfinite"/>. If not
    ///     then use appropriate slice of the byte buffer to decode a value of T to use as the
    ///     starting bound. Next, check the flags value to see if it contains the
    ///     <see cref="RangeFlag.LowerBoundInclusive"/>. If yes, then lower bound is
    ///     <see cref="Bound{T}.Included"/>. Otherwise, the lower bound is
    ///     <see cref="Bound{T}.Excluded"/>.
    ///     </item>
    ///     <item>
    ///     Check if the flags value contains the <see cref="RangeFlag.UpperBoundInfinite"/>. If not
    ///     then use appropriate slice of the byte buffer to decode a value of T to use as the
    ///     final bound. Next, check the flags value to see if it contains the
    ///     <see cref="RangeFlag.UpperBoundInclusive"/>. If yes, then upper bound is
    ///     <see cref="Bound{T}.Included"/>. Otherwise, the upper bound is
    ///     <see cref="Bound{T}.Excluded"/>.
    ///     </item>
    /// </list>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/rangetypes.c#L261">pg source code</a>
    /// </summary>
    [SuppressMessage("ReSharper", "InvertIf")]
    public static PgRange<TValue> DecodeBytes(ref PgBinaryValue value)
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
            var columnMetadata = PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary);
            var lowerBoundValue = new PgBinaryValue(
                value.Buffer.Slice(lowerBoundLength),
                ref columnMetadata);
            TValue lowerValue = TType.DecodeBytes(ref lowerBoundValue);
            start = flags.HasFlag(RangeFlag.LowerBoundInclusive)
                ? Bound<TValue>.Included(lowerValue)
                : Bound<TValue>.Excluded(lowerValue);
        }
            
        if (!flags.HasFlag(RangeFlag.UpperBoundInfinite))
        {
            var upperBoundLength = value.Buffer.ReadInt();
            var columnMetadata = PgColumnMetadata.CreateMinimal(TType.DbType, PgFormatCode.Binary);
            var upperBoundValue = new PgBinaryValue(
                value.Buffer.Slice(upperBoundLength),
                ref columnMetadata);
            TValue upperValue = TType.DecodeBytes(ref upperBoundValue);
            end = flags.HasFlag(RangeFlag.UpperBoundInclusive)
                ? Bound<TValue>.Included(upperValue)
                : Bound<TValue>.Excluded(upperValue);
        }

        return new PgRange<TValue>(start, end);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Strip the bound characters from the character buffer and provide that resulting value to
    /// <see cref="FindRangeSeparatorIndex"/> to get the index of the range separator character.
    /// The characters are then sliced for bound of the range and passed to the inner type
    /// <typeparamref name="TType"/> to decode and interpret as an inclusive or exclusive bound.
    /// If either range value is empty/null, default to <see cref="Bound{T}.Unbounded"/>. After the
    /// 2 bounds have been decoded, combine into a new <see cref="PgRange{T}"/> instance.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/rangetypes.c#L137">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the number of bounds in the range literal is > 2 or decoding a bound value fails
    /// </exception>
    [SuppressMessage("ReSharper", "InvertIf")]
    public static PgRange<TValue> DecodeText(PgTextValue value)
    {
        var start = Bound<TValue>.Unbounded();
        var end = Bound<TValue>.Unbounded();
        
        PgTextValue validChars = value.Slice(1..^1);
        var separatorIndex = FindRangeSeparatorIndex(validChars);
        if (separatorIndex == -1)
        {
            throw ColumnDecodeException.Create<PgRange<TValue>>(
                value.ColumnMetadata,
                $"Could not find separator character in '{value}'");
        }
        
        if (separatorIndex != 0)
        {
            PgTextValue startSlice = validChars.Slice(..separatorIndex);
            TValue lowerBoundValue = TType.DecodeText(startSlice);
            start = DecodeBound(value.Chars[0], lowerBoundValue, value.ColumnMetadata);
        }

        if (separatorIndex != validChars.Chars.Length - 1)
        {
            PgTextValue endSlice = validChars.Slice((separatorIndex + 1)..);
            TValue upperBoundValue = TType.DecodeText(endSlice);
            end = DecodeBound(value.Chars[^1], upperBoundValue, value.ColumnMetadata);
        }
        
        return new PgRange<TValue>(start, end);
    }

    public static PgType DbType => TType.RangeType;

    public static PgType ArrayDbType => TType.RangeArrayType;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType == DbType;
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
        return -1;
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
            _ => throw ColumnDecodeException.Create<PgRange<TValue>>(
                columnMetadata,
                $"Illegal bound character '{chr}'"),
        };
    }
}

// https://github.com/postgres/postgres/blob/master/src/include/utils/rangetypes.h#L38-L45
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
