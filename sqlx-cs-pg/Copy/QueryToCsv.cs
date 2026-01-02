using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying to STDOUT as CSV data extracted from the
/// query specified
/// </summary>
public record QueryToCsv : ICopyQuery, ICopyCsv
{
    public required string Query { get; init; }

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
    
    /// <summary>
    /// Forces quoting to be used for all non-NULL values in each specified column. NULL output is
    /// never quoted. If <see cref="ForceAgainstColumns.All"/> is specified, non-NULL values will be
    /// quoted in all columns. This option is allowed only when <see cref="ICopyTo"/>, and only when
    /// using <see cref="CopyFormat.Csv"/>, otherwise the value is ignored.
    /// </summary>
    public ForceAgainstColumns? ForceQuote { get; init; }

    public string ToCopyQuery()
    {
        StringBuilder builder = new("COPY (");
        builder.Append(Query).Append(") TO STDOUT ");
        this.AppendCsvOptions(builder);
        ForceQuote?.AppendForceQuoteTo(builder);
        builder.Append(')');
        return builder.ToString();
    }
}
