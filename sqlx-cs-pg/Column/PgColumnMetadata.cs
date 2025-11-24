using Sqlx.Core.Column;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Column;

/// <summary>
/// Column metadata from a postgres query description
/// </summary>
/// <param name="FieldName">Field name</param>
/// <param name="TableOid">OID of the table (0 if the field is not part of a table)</param>
/// <param name="ColumnAttribute">
/// Attribute number of the field (0 if the field not part of a table)
/// </param>
/// <param name="PgTypeInfo">Type info for the field</param>
/// <param name="DataTypeSize">
/// Size of the data type. Negative values denote a variables width type
/// </param>
/// <param name="TypeModifier">
/// Modifier of the data type (see
/// <a href="https://www.postgresql.org/docs/current/catalog-pg-attribute.html">pg_attribute</a>).
/// Will be -1 when the type does not need <c>atttypmod</c>
/// </param>
/// <param name="FormatCode">Format code of the field</param>
public readonly record struct PgColumnMetadata(
    string FieldName,
    int TableOid,
    int ColumnAttribute,
    PgTypeInfo PgTypeInfo,
    short DataTypeSize,
    int TypeModifier,
    PgFormatCode FormatCode) : IColumnMetadata
{
    public uint DataType { get; } = PgTypeInfo.TypeOid.Inner;

    /// <summary>
    /// Copies the current column metadata with binary format specified
    /// </summary>
    /// <returns>new metadata set with binary format</returns>
    public PgColumnMetadata WithBinaryFormat()
    {
        return new PgColumnMetadata(
            FieldName,
            TableOid,
            ColumnAttribute,
            PgTypeInfo,
            DataTypeSize,
            TypeModifier,
            PgFormatCode.Binary);
    }

    /// <summary>
    /// Creates a new column metadata set where all values expect for the supplied values are
    /// default (0 or empty string).
    /// </summary>
    /// <param name="pgType">Column data type</param>
    /// <param name="formatCode">Column data format code</param>
    /// <returns>new metadata with specified values and defaults</returns>
    public static PgColumnMetadata CreateMinimal(PgTypeInfo pgType, PgFormatCode formatCode)
    {
        return new PgColumnMetadata("", 0, 0, pgType, 0, 0, formatCode);
    }
}
