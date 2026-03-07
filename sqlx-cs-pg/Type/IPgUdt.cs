namespace Sqlx.Postgres.Type;

/// <summary>
/// 
/// </summary>
public interface IPgUdt<T> : IPgDbType<T> where T : notnull
{
    /// <summary>
    /// <see cref="PgTypeInfo"/> definition for this type. Allow for setting the property to update
    /// with database specific type info. Users should NOT set this value
    /// </summary>
    new static abstract PgTypeInfo DbType { get; set; }

    /// <summary>
    /// Well known name of the type
    /// </summary>
    static abstract string TypeName { get; }
}
