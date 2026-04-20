using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying from STDIN as binary data into the table
/// specified
/// </summary>
public record CopyTableFromBinary : ICopyFrom, ICopyTable, ICopyBinary
{
    public required string SchemaName { get; init; }

    public required string TableName { get; init; }

    public IReadOnlyList<string> ColumnNames { get; init; } = [];

    public string ToCopyQuery()
    {
        StringBuilder builder = new("COPY ");
        this.AppendTableDetailsTo(builder);
        builder.Append(" FROM STDIN WITH (FORMAT binary)");
        return builder.ToString();
    }
}
