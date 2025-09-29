namespace Sqlx.Postgres.Type;

/// <summary>
/// Defines a type as having an array type. Technically every postgres type has an array type but
/// not all types truly need to specify their array variant (especially user defined types where
/// arrays are less likely to be used). This should be implemented on a <see cref="IPgDbType{T}"/>
/// to have any effect.
/// </summary>
public interface IHasArrayType
{
    /// <summary>
    /// <see cref="PgType"/> for the array super type of another database type.
    /// </summary>
    static abstract PgType ArrayDbType { get; }
}
