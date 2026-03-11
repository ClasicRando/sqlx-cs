using System.Buffers;
using System.Runtime.CompilerServices;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>MONEY</c> type represented as a 64-bit integer. This covers monetary values without
/// using floating point or decimal values.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-money.html">docs</a>
/// </summary>
public readonly struct PgMoney : IPgDbType<PgMoney>, IHasArrayType, IEquatable<PgMoney>
{
    private static readonly SearchValues<char> SearchValues = System.Buffers.SearchValues.Create("0123456789.-");
    private readonly long _inner;
    
    private PgMoney(long integer)
    {
        _inner = integer;
    }

    public PgMoney(decimal value)
    {
        if (value.Scale > 2)
        {
            throw new ArgumentException(
                "Decimal values with more than 2 Scale would be truncated thus losing precision",
                nameof(value));
        }

        _inner = decimal.ToInt64(value * 100);
    }

    public PgMoney(double value) : this(Convert.ToDecimal(value)) {}

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the money value as a <see cref="long"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/cash.c#L513">pg source code</a>
    /// </summary>
    public static void Encode(PgMoney value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.WriteLong(value._inner);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Reads a <see cref="long"/> value to use as the inner value of a <see cref="PgMoney"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/cash.c#L524">pg source code</a>
    /// </summary>
    public static PgMoney DecodeBytes(in PgBinaryValue value)
    {
        var buff = value.Buffer;
        return new PgMoney(buff.ReadLong());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Parses the characters to extract only digits, minus signs and decimal places. Any character
    /// such as '$' will be ignored while parsing. Extracts to a <see cref="decimal"/> value that is
    /// later converted to a <see cref="long"/> value with 0 scale.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/cash.c#L310">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters cannot be parsed into a decimal value
    /// </exception>
    public static PgMoney DecodeText(in PgTextValue value)
    {
        if (value.Chars.ContainsAnyExcept(SearchValues))
        {
            var tempSpan = value.Chars.Length > 128
                ? new char[value.Chars.Length]
                : stackalloc char[value.Chars.Length];
            var charCount = 0;
            foreach (var chr in value.Chars)
            {
                if (char.IsDigit(chr) || chr == '.')
                {
                    tempSpan[charCount++] = chr;
                }
            }
            
            if (decimal.TryParse(tempSpan[..charCount], null, out var result))
            {
                return new PgMoney(result);
            }
        }
        else
        {
            if (decimal.TryParse(value.Chars, null, out var result))
            {
                return new PgMoney(result);
            }
        }
            
        throw ColumnDecodeException.Create<PgMoney, PgColumnMetadata>(
            value.ColumnMetadata,
            $"Could not parse '{value.Chars}' into a money value");
    }

    public static PgTypeInfo DbType => PgTypeInfo.Money;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.MoneyArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    public override string ToString()
    {
        const decimal oneHundred = 100;
        var decimalValue = _inner / oneHundred;
        return $"{(_inner < 0 ? "-" : string.Empty)}${decimalValue}";
    }

    public bool Equals(PgMoney other)
    {
        return _inner == other._inner;
    }

    public override bool Equals(object? obj)
    {
        return obj is PgMoney other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _inner.GetHashCode();
    }

    public static bool operator ==(PgMoney left, PgMoney right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgMoney left, PgMoney right)
    {
        return !(left == right);
    }

    public static PgMoney operator +(PgMoney left, PgMoney right)
    {
        return left.Add(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PgMoney Add(PgMoney other)
    {
        return new PgMoney(_inner + other._inner);
    }

    public static PgMoney operator -(PgMoney left, PgMoney right)
    {
        return left.Subtract(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PgMoney Subtract(PgMoney other)
    {
        return new PgMoney(_inner - other._inner);
    }
}
