using System.Text;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>RANGE</c> types represented as a lower and upper bound over a type
/// <typeparamref name="T"/>
/// </para>
/// <a href="https://www.postgresql.org/docs/current/rangetypes.html">docs</a>
/// </summary>
/// <param name="Lower">Lower bound of the range</param>
/// <param name="Upper">Upper bound of the range</param>
/// <typeparam name="T">Range type</typeparam>
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

/// <summary>
/// Bound description of a <see cref="PgRange{T}"/>. You cannot directly construct an instance of
/// this type. Rather, there are static factory methods <see cref="Included"/>,
/// <see cref="Excluded"/> and <see cref="Unbounded"/> that create an instance of the desired
/// <see cref="BoundType"/>. This ensures the consistency of the instances of this class. 
/// </summary>
/// <typeparam name="T">Range type</typeparam>
public sealed class Bound<T> : IEquatable<Bound<T>> where T : notnull
{
    public T? Value { get; }
    public BoundType Type { get; }
    
    private Bound(T? value, BoundType type)
    {
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Create a new instance with value specified as an inclusive bound (range includes this value)
    /// </summary>
    /// <param name="value">Range bound value</param>
    /// <returns>Inclusive range bound</returns>
    public static Bound<T> Included(T value)
    {
        return new Bound<T>(value, BoundType.Included);
    }

    /// <summary>
    /// Create a new instance with value specified as an exclusive bound (range does not include
    /// this value)
    /// </summary>
    /// <param name="value">Range bound value</param>
    /// <returns>Exclusive range bound</returns>
    public static Bound<T> Excluded(T value)
    {
        return new Bound<T>(value, BoundType.Excluded);
    }

    /// <summary>
    /// Create a new undefined/infinite bound on a range
    /// </summary>
    /// <returns>Unbounded range bound</returns>
    public static Bound<T> Unbounded()
    {
        return new Bound<T>(default, BoundType.Unbounded);
    }

    public override bool Equals(object? obj)
    {
        return obj is Bound<T> other && Equals(other);
    }

    public bool Equals(Bound<T>? other)
    {
        if (other is null)
        {
            return false;
        }
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
