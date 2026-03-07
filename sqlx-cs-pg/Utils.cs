using System.Text;

namespace Sqlx.Postgres;

internal static class Utils
{
    extension(string str)
    {
        internal string QuoteIdentifier()
        {
            return $"\"{str.Replace("\"", "\"\"", StringComparison.InvariantCulture)}\"";
        }
    }
    
    extension(StringBuilder builder)
    {
        internal StringBuilder AppendQuotedIdentifier(string identifier)
        {
            return builder.Append('"')
                .Append(identifier.Replace("\"", "\"\"", StringComparison.InvariantCulture))
                .Append('"');
        }
    }

    extension(IReadOnlyList<string> strings)
    {
        internal void JoinTo(
            StringBuilder builder,
            string separator,
            string prefix = "",
            string postFix = "",
            Func<StringBuilder, string, StringBuilder>? append = null)
        {
            if (strings.Count == 0)
            {
                return;
            }
            
            builder.Append(prefix);
            for (var i = 0; i < strings.Count; i++)
            {
                var item = strings[i];
                if (append is not null)
                {
                    append(builder, item);
                }
                else
                {
                    builder.Append(item);
                }
                if (i != strings.Count - 1)
                {
                    builder.Append(separator);
                }
            }
            builder.Append(postFix);
        }
    }
}
