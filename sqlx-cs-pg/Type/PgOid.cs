namespace Sqlx.Postgres.Type;

public readonly struct PgOid(int inner) : IEquatable<PgOid>
{
    public int Inner { get; } = inner;

    public static implicit operator int(PgOid pgOid) => pgOid.Inner;

    public static implicit operator PgOid(int oid) => new(oid);

    public bool Equals(PgOid other)
    {
        return Inner == other.Inner;
    }

    public override bool Equals(object? obj)
    {
        return obj is PgOid other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Inner;
    }

    public override string ToString()
    {
        return $"{nameof(PgOid)}({Inner})";
    }

    public static bool operator ==(PgOid left, PgOid right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgOid left, PgOid right)
    {
        return !left.Equals(right);
    }
}
