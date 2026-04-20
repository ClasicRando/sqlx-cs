using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying to STDOUT as text delimited data
/// extracted from the table specified
/// </summary>
public record CopyTableToText : ICopyTo, ICopyTable, ICopyText
{
    public required string SchemaName { get; init; }

    public required string TableName { get; init; }

    public IReadOnlyList<string> ColumnNames { get; init; } = [];

    public char Delimiter { get; init; } = '\t';

    public string NullString { get; init; } = "\\N";

    public string? DefaultValue { get; init; }

    public CopyHeader? Header { get; init; }

    public string ToCopyQuery()
    {
        StringBuilder builder = new();
        builder.Append("COPY ");
        this.AppendTableDetailsTo(builder);
        builder.Append(" TO STDOUT");
        this.AppendTextOptionsTo(builder);
        return builder.ToString();
    }
}
