using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying from STDIN as CSV data into the table
/// specified
/// </summary>
public record TableFromCsv : ICopyFrom, ICopyTable, ICopyCsv
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

    /// <summary>
    /// Do not match the specified columns' values against the null string. In the default case
    /// where the null string is empty, this means that empty values will be read as zero-length
    /// strings rather than nulls, even when they are not quoted.
    /// </summary>
    public ForceAgainstColumns.Select? ForceNotNull { get; init; }
    
    /// <summary>
    /// Match the specified columns' values against the null string, even if it has been quoted, and
    /// if a match is found set the value to NULL. In the default case where the null string is
    /// empty, this converts a quoted empty string into NULL.
    /// </summary>
    public ForceAgainstColumns.Select? ForceNull { get; init; }

    public string ToCopyQuery()
    {
        StringBuilder builder = new("COPY ");
        this.AppendTableDetailsTo(builder);
        builder.Append(" FROM STDIN");
        this.AppendCsvOptions(builder);
        ForceNotNull?.AppendForceNotNullTo(builder);
        ForceNull?.AppendForceNullTo(builder);
        builder.Append(')');
        return builder.ToString();
    }
}
