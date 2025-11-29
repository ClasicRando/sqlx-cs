namespace Sqlx.Postgres.Type;

/// <summary>
/// Extension interface for <see cref="IPgDbType{T}"/> to allow for setting the <see cref="DbType"/>
/// property (needed to ensure type name resolves to database instance specific OID) based upon the
/// known <see cref="TypeName"/>.
/// </summary>
/// <typeparam name="TSelf">This type</typeparam>
public interface IPgUdt<TSelf> : IPgDbType<TSelf> where TSelf : notnull
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
