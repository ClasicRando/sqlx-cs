using System.Text;

namespace Sqlx.Postgres.Type;

public record PgRange<T>(Bound<T> Lower, Bound<T> Upper) where T : notnull
{
    private readonly Lazy<string> _postgresLiteral = new(
        () =>
        {
            var builder = new StringBuilder();
            switch (Lower.Type)
            {
                case BoundType.Included:
                    builder.Append('(');
                    builder.Append(Lower.Value);
                    break;
                case BoundType.Excluded:
                    builder.Append('[');
                    builder.Append(Lower.Value);
                    break;
                case BoundType.Unbounded:
                    builder.Append('(');
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            builder.Append(',');
            
            switch (Upper.Type)
            {
                case BoundType.Included:
                    builder.Append(Upper.Value);
                    builder.Append(')');
                    break;
                case BoundType.Excluded:
                    builder.Append(Upper.Value);
                    builder.Append(']');
                    break;
                case BoundType.Unbounded:
                    builder.Append(')');
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return builder.ToString();
        });

    public string PostgresLiteral => _postgresLiteral.Value;
}

public class Bound<T> where T : notnull
{
    public T? Value { get; }
    public BoundType Type { get; }
    
    private Bound(T? value, BoundType type)
    {
        Value = value;
        Type = type;
    }

    public static Bound<T> Included(T value)
    {
        return new Bound<T>(value, BoundType.Included);
    }

    public static Bound<T> Excluded(T value)
    {
        return new Bound<T>(value, BoundType.Excluded);
    }

    public static Bound<T> Unbounded()
    {
        return new Bound<T>(default, BoundType.Unbounded);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Bound<T> other) return false;
        if (Type is BoundType.Unbounded && other.Type is BoundType.Unbounded)
        {
            return true;
        }
        return Value is not null && Value.Equals(other.Value) && Type == other.Type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, (int)Type);
    }
    
    public override string ToString()
    {
        return $"{nameof(Bound<T>)}({nameof(Value)}: {Value}, {nameof(Type)}: {Type})";
    }
}
    
public enum BoundType
{
    Included,
    Excluded,
    Unbounded,
}
