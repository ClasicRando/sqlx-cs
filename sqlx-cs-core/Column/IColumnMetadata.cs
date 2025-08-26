namespace Sqlx.Core.Column;

public interface IColumnMetadata
{
    string FieldName { get; }
    // string TypeName { get; }
    int DataType { get; }
}
