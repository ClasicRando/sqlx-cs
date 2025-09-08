namespace Sqlx.Postgres.Type;

internal interface IHasRangeType
{
    static abstract PgType RangeType { get; }
    
    static abstract PgType RangeArrayType { get; }
}
