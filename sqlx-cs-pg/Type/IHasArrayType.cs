namespace Sqlx.Postgres.Type;

public interface IHasArrayType
{
    static abstract PgType ArrayDbType { get; }
}
