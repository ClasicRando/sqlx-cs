using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <c>COPY</c> statement that specifies a text based formatting
/// </summary>
public interface ICopyText : ICopyStatement
{
    /// <summary>
    /// Specifies the character that separates columns within each row of the file. The default
    /// character is tab when in <see cref="CopyFormat.Text"/> and comma when
    /// <see cref="CopyFormat.Csv"/>.
    /// </summary>
    char Delimiter { get; }
    
    /// <summary>
    /// Specifies the string that represents a null value. The default is "\N" (backslash-N) in
    /// <see cref="CopyFormat.Text"/> format, and an unquoted empty string in
    /// <see cref="CopyFormat.Csv"/>. You might prefer an empty string even in text format for cases
    /// where you don't want to distinguish nulls from empty strings.
    /// </summary>
    string NullString { get; }
    
    /// <summary>
    /// Specifies the string that represents a default value. Each time the string is found in the
    /// input file, the default value of the corresponding column will be used. This option is
    /// ignored unless the type is <see cref="ICopyFrom"/>.
    /// </summary>
    string? Default { get; }
    
    /// <summary>
    /// Specifies that the file contains a header line with the names of each column in the file.
    /// On output, the first line contains the column names from the table. On input, the first line
    /// is discarded when this option is set to <see cref="CopyHeader.True"/>. If this option is set
    /// to <see cref="CopyHeader.Match"/>, the number and names of the columns in the header line
    /// must match the actual column names of the table, in order; otherwise an error is raised.
    /// This option is ignored when <see cref="CopyFormat.Binary"/>. The
    /// <see cref="CopyHeader.Match"/> option is only valid for <see cref="ICopyFrom"/>.
    /// </summary>
    CopyHeader? Header { get; }
}

internal static class CopyText
{
    extension(ICopyText copyText)
    {
        internal void AppendTextOptionsTo(StringBuilder builder)
        {
            builder.Append(" WITH (FORMAT text, DELIMITER '")
                .Append(copyText.Delimiter == '\'' ? "''" : copyText.Delimiter)
                .Append("', NULL '")
                .Append(copyText.NullString.Replace("'", "''"))
                .Append('\'');
            copyText.AppendDefaultOptionTo(builder);
            copyText.AppendHeaderOptionTo(builder);
            builder.Append(')');
        }

        internal void AppendDefaultOptionTo(StringBuilder builder)
        {
            if (copyText.Default is null)
            {
                return;
            }
            
            builder.Append(", DEFAULT '")
                .Append(copyText.Default.Replace("'", "''"))
                .Append('\'');
        }
        
        internal void AppendHeaderOptionTo(StringBuilder builder)
        {
            if (copyText.Header is null)
            {
                return;
            }

            builder.Append(", DEFAULT '");
            switch (copyText.Header)
            {
                case CopyHeader.True:
                    builder.Append("true");
                    break;
                case CopyHeader.False:
                    builder.Append("false");
                    break;
                case CopyHeader.Match:
                    builder.Append("MATCH");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ICopyText.Header), copyText.Header, null);
            }

            builder.Append('\'');
        }
    }
}
