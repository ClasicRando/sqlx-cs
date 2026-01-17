using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> implementation for copying to STDOUT as text delimited data
/// extracted from the query specified.
/// </summary>
public record QueryToText : ICopyQuery, ICopyText
{
    public required string Query { get; init; }

    public char Delimiter { get; init; } = '\t';

    public string NullString { get; init; } = "\\N";
    
    public string? DefaultValue { get; init; }
    
    public CopyHeader? Header { get; init; }
    
    public string ToCopyQuery()
    {
        StringBuilder builder = new();
        builder.Append("COPY (").Append(Query).Append(") TO STDOUT");
        this.AppendTextOptionsTo(builder);
        return builder.ToString();
    }
}
