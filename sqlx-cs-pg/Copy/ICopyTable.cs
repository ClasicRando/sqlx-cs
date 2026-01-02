using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// <see cref="ICopyStatement"/> where the target of the <c>COPY</c> statement is a table (either
/// copying to or from)
/// </summary>
public interface ICopyTable : ICopyStatement
{
    /// <summary>
    /// Schema of the target table
    /// </summary>
    string SchemaName { get; }

    /// <summary>
    /// Target table of the COPY operation. Must already exist
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Optional column names that will be copied. If no column list is specified then all columns
    /// of the table except for the generated columns will be copied.
    /// </summary>
    string[] ColumnNames { get; }
}

internal static class CopyTable
{
    extension(ICopyTable copyTable)
    {
        public void AppendTableDetailsTo(StringBuilder builder)
        {
            builder.AppendQuotedIdentifier(copyTable.SchemaName)
                .Append('.')
                .AppendQuotedIdentifier(copyTable.TableName);

            if (copyTable.ColumnNames.Length == 0)
            {
                return;
            }

            copyTable.ColumnNames.JoinTo(
                builder,
                separator: ",",
                prefix: "(",
                postFix: ")",
                append: Utils.AppendQuotedIdentifier);
        }
    }
}
