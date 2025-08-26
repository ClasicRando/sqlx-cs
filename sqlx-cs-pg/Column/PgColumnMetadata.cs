using Sqlx.Core.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Column;

public record PgColumnMetadata(
    string FieldName,
    int TableOid,
    int ColumnAttribute,
    PgType PgType,
    short DataTypeSize,
    int TypeModifier,
    PgFormatCode FormatCode) : IColumnMetadata
{
    public int DataType { get; } = PgType.TypeOid;

    public PgColumnMetadata WithBinaryFormat()
    {
        return new PgColumnMetadata(
            FieldName,
            TableOid,
            ColumnAttribute,
            PgType,
            DataTypeSize,
            TypeModifier,
            PgFormatCode.Binary);
    }

    public static PgColumnMetadata CreateMinimal(PgType pgType, PgFormatCode formatCode)
    {
        return new PgColumnMetadata("", 0, 0, pgType, 0, 0, formatCode);
    }
}
