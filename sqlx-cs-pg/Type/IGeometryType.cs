namespace Sqlx.Postgres.Type;

/// <summary>
/// Implementors are simple Postgres geometry types that expose a readonly property as the geometry
/// types literal representation
/// </summary>
public interface IGeometryType
{
    /// <summary>
    /// Literal representation of the geometry value
    /// </summary>
    string GeometryLiteral { get; }
}
