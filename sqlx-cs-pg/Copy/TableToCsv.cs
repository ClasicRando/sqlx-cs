using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying to STDOUT as CSV data extracted from the
/// table specified
/// </summary>
public record TableToCsv : ICopyTo, ICopyTable, ICopyCsv
{
    public required string SchemaName { get; init; }

    public required string TableName { get; init; }

    public string[] ColumnNames { get; init; } = [];

    public char Delimiter { get; init; } = ',';

    public string NullString { get; init; } = "";

    public string? Default { get; init; }

    public CopyHeader? Header { get; init; }

    public char Quote { get; init; } = '"';

    public char Escape
    {
        get => field == '\0' ? Quote : field;
        init;
    }

    public ForceAgainstColumns? ForceQuote { get; init; }

    public string ToCopyQuery()
    {
        StringBuilder builder = new("COPY ");
        this.AppendTableDetailsTo(builder);
        builder.Append(" TO STDOUT");
        this.AppendCsvOptions(builder);
        ForceQuote?.AppendForceQuoteTo(builder);
        builder.Append(')');
        return builder.ToString();
    }
}
