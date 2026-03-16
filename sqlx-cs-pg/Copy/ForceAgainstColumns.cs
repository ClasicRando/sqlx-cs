using System.Text;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// All possible options for forcing columns to follow some behaviour. The only allowed subclasses
/// are <see cref="SelectNames"/> with specific named columns and <see cref="All"/> where all columns are
/// given the forced behaviour.
/// </summary>
public abstract record ForceAgainstColumns
{
    private ForceAgainstColumns()
    {
    }

    public record SelectNames(IReadOnlyList<string> Columns) : ForceAgainstColumns;

    public record All : ForceAgainstColumns;

    public void AppendForceQuoteTo(StringBuilder builder)
    {
        AppendTo(builder, "QUOTE");
    }

    public void AppendForceNullTo(StringBuilder builder)
    {
        AppendTo(builder, "NULL");
    }

    public void AppendForceNotNullTo(StringBuilder builder)
    {
        AppendTo(builder, "NOT_NULL");
    }

    private void AppendTo(StringBuilder builder, string name)
    {
        ArgumentNullException.ThrowIfNull(builder);
        switch (this)
        {
            case All:
                AppendOptionName(builder, name);
                builder.Append('*');
                break;
            case SelectNames select:
                if (select.Columns.Count == 0)
                {
                    return;
                }

                AppendOptionName(builder, name);
                select.Columns.JoinTo(
                    builder,
                    separator: ",",
                    append: Utils.AppendQuotedIdentifier);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "This type is an unexpected extension of ForceAgainstColumns. Why would you do this?",
                    innerException: null);
        }
    }

    private static void AppendOptionName(StringBuilder builder, string name)
    {
        builder.Append(", FORCE_")
            .Append(name)
            .Append(' ');
    }
}
