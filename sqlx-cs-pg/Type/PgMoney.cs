using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgMoney : IPgDbType<PgMoney>
{
    private readonly long _inner;
    
    internal PgMoney(long integer)
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

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgMoney value)
    {
        return DbType;
    }
}
