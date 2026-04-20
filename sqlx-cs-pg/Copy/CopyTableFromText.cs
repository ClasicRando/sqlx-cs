using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying from STDIN as text data into the table
/// specified
/// </summary>
public record CopyTableFromText : ICopyFrom, ICopyTable, ICopyText
{
    public required string SchemaName { get; init; }

    public required string TableName { get; init; }

    public IReadOnlyList<string> ColumnNames { get; init; } = [];

    public char Delimiter { get; init; } = ',';

    public string NullString { get; init; } = "";

    public string? DefaultValue { get; init; }

    public CopyHeader? Header { get; init; }

    public string ToCopyQuery()
    {
        StringBuilder builder = new("COPY ");
        this.AppendTableDetailsTo(builder);
        builder.Append(" FROM STDIN");
        this.AppendTextOptionsTo(builder);
        return builder.ToString();
    }
}
