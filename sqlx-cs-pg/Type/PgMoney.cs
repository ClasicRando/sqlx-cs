using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly struct PgMoney : IPgDbType<PgMoney>, IHasArrayType, IEquatable<PgMoney>
{
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

    public static void Encode(PgMoney value, WriteBuffer buffer)
    {
        buffer.WriteLong(value._inner);
    }

    public static PgMoney DecodeBytes(PgBinaryValue value)
    {
        return new PgMoney(value.Buffer.ReadLong());
    }

    public static PgMoney DecodeText(PgTextValue value)
    {
        Span<char> tempSpan = stackalloc char[value.Chars.Length];
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
        throw ColumnDecodeError.Create<PgMoney>(
            value.ColumnMetadata,
            $"Could not parse '{value}' into a money value");
    }

    public static PgType DbType => PgType.Money;

    public static PgType ArrayDbType => PgType.MoneyArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgMoney value)
    {
        return DbType;
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
        return new PgMoney(left._inner + right._inner);
    }

    public static PgMoney operator -(PgMoney left, PgMoney right)
    {
        return new PgMoney(left._inner - right._inner);
    }
}
