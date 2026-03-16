using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying to STDOUT as binary data extracted from
/// the table specified
/// </summary>
public record CopyTableToBinary : ICopyTo, ICopyTable, ICopyBinary
{
    public required string SchemaName { get; init; }

    public required string TableName { get; init; }

    public IReadOnlyList<string> ColumnNames { get; init; } = [];

    public string ToCopyQuery()
    {
        StringBuilder builder = new("COPY ");
        this.AppendTableDetailsTo(builder);
        builder.Append(" TO STDOUT");
        builder.Append(" WITH (FORMAT binary)");
        return builder.ToString();
    }
}
