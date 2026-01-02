using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// Specialized <see cref="ICopyText"/> statement that uses the CSV format rather than a general
/// text format
/// </summary>
public interface ICopyCsv : ICopyText
{
    /// <summary>
    /// Specifies the quoting character to be used when a data value is quoted. The default is
    /// double-quote. This must be a single one-byte character.
    /// </summary>
    char Quote { get; }
    
    /// <summary>
    /// Specifies the character that should appear before a data character that matches the
    /// <see cref="Quote"/> value. The default is the same as the <see cref="Quote"/> value (so that
    /// the quoting character is doubled if it appears in the data).
    /// </summary>
    char Escape { get; }
}

public static class CopyCsv
{
    extension(ICopyCsv copyCsv)
    {
        internal void AppendCsvOptions(StringBuilder builder)
        {
            builder.Append(" WITH (FORMAT csv, DELIMITER '")
                .Append(copyCsv.Delimiter == '\'' ? "''" : copyCsv.Delimiter)
                .Append("', NULL '")
                .Append(copyCsv.NullString.Replace("'", "''"))
                .Append('\'');
            copyCsv.AppendDefaultOptionTo(builder);
            copyCsv.AppendHeaderOptionTo(builder);
            builder.Append(", QUOTE '")
                .Append(copyCsv.Quote == '\'' ? "''" : copyCsv.Quote)
                .Append("', ESCAPE '")
                .Append(copyCsv.Escape == '\'' ? "''" : copyCsv.Escape)
                .Append('\'');
        }
    }
}
