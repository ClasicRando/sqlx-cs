using System.Text;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>RANGE</c> types represented as a lower and upper bound over a type
/// <typeparamref name="T"/>
/// </para>
/// <a href="https://www.postgresql.org/docs/current/rangetypes.html">docs</a>
/// </summary>
/// <param name="lower">Lower bound of the range</param>
/// <param name="upper">Upper bound of the range</param>
/// <typeparam name="T">Range type</typeparam>
public sealed class PgRange<T>(Bound<T> lower, Bound<T> upper) : IEquatable<PgRange<T>>
    where T : notnull
{
    private readonly Lazy<string> _postgresLiteral = new(() =>
    {
        var builder = new StringBuilder();
        switch (lower.Type)
        {
            case BoundType.Included:
                builder.Append('[');
                builder.Append(lower.Value);
                break;
            case BoundType.Excluded:
                builder.Append('(');
                builder.Append(lower.Value);
                break;
            case BoundType.Unbounded:
                builder.Append('(');
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        builder.Append(',');

        switch (upper.Type)
        {
            case BoundType.Included:
                builder.Append(upper.Value);
                builder.Append(']');
                break;
            case BoundType.Excluded:
                builder.Append(upper.Value);
                builder.Append(')');
                break;
            case BoundType.Unbounded:
                builder.Append(')');
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return builder.ToString();
    });

    /// <summary>
    /// Lower bound of the range
    /// </summary>
    public Bound<T> Lower { get; } = lower;

    /// <summary>
    /// Upper bound of the range
    /// </summary>
    public Bound<T> Upper { get; } = upper;

    public string PostgresLiteral => _postgresLiteral.Value;

    public bool Equals(PgRange<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Lower.Equals(other.Lower) && Upper.Equals(other.Upper);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgRange<T> range && Equals(range);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Lower, Upper);
    }

    public static bool operator ==(PgRange<T>? left, PgRange<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PgRange<T>? left, PgRange<T>? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Bound description of a <see cref="PgRange{T}"/>. You cannot directly construct an instance of
/// this type. Rather, there are static factory methods <see cref="Bound.Included"/>,
/// <see cref="Bound.Excluded"/> and <see cref="Bound.Unbounded"/> that create an instance of the
/// desired <see cref="BoundType"/>. This ensures the consistency of the instances of this class. 
/// </summary>
/// <typeparam name="T">Range type</typeparam>
public sealed class Bound<T> : IEquatable<Bound<T>> where T : notnull
{
    public T? Value { get; }
    public BoundType Type { get; }

    internal Bound(T? value, BoundType type)
    {
        Value = value;
        Type = type;
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

public static class Bound
{
    /// <summary>
    /// Create a new instance with value specified as an inclusive bound (range includes this value)
    /// </summary>
    /// <param name="value">Range bound value</param>
    /// <returns>Inclusive range bound</returns>
    public static Bound<T> Included<T>(T value) where T : notnull
    {
        return new Bound<T>(value, BoundType.Included);
    }

    /// <summary>
    /// Create a new instance with value specified as an exclusive bound (range does not include
    /// this value)
    /// </summary>
    /// <param name="value">Range bound value</param>
    /// <returns>Exclusive range bound</returns>
    public static Bound<T> Excluded<T>(T value) where T : notnull
    {
        return new Bound<T>(value, BoundType.Excluded);
    }

    /// <summary>
    /// Create a new undefined/infinite bound on a range
    /// </summary>
    /// <returns>Unbounded range bound</returns>
    public static Bound<T> Unbounded<T>() where T : notnull
    {
        return new Bound<T>(default, BoundType.Unbounded);
    }
}

public enum BoundType
{
    Included,
    Excluded,
    Unbounded,
}
