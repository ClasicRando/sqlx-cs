namespace Sqlx.Core.Column;

/// <summary>
/// Minimal information needed to represent a column from any database driver
/// </summary>
public interface IColumnMetadata
{
    /// <summary>
    /// Name of the column
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Database vendor specific code of the column's data type
    /// </summary>
    uint DataType { get; }
}
