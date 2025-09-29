namespace Sqlx.Postgres.Type;

/// <summary>
/// Indicates that a type can be used in a <see cref="PgRange{T}"/>. Although any type can
/// technically be used within ranges, only <see cref="IPgDbType{T}"/> definitions that implement
/// this interface can be encoded/decoded by <see cref="PgRangeType{TValue,TType}"/> .
/// </summary>
internal interface IHasRangeType
{
    /// <summary>
    /// <see cref="PgType"/> of this type's range
    /// </summary>
    static abstract PgType RangeType { get; }
    
    /// <summary>
    /// <see cref="PgType"/> of this type's array range
    /// </summary>
    static abstract PgType RangeArrayType { get; }
}
