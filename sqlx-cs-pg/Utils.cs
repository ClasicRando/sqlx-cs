using System.Text;

namespace Sqlx.Postgres;

internal static class Utils
{
    extension(string str)
    {
        internal string QuoteIdentifier()
        {
            return $"\"{str.Replace("\"", "\"\"")}\"";
        }
    }
    
    extension(StringBuilder builder)
    {
        internal StringBuilder AppendQuotedIdentifier(string identifier)
        {
            return builder.Append('"')
                .Append(identifier.Replace("\"", "\"\""))
                .Append('"');
        }
    }

    extension(string[] strings)
    {
        internal void JoinTo(
            StringBuilder builder,
            string separator,
            string prefix = "",
            string postFix = "",
            Func<StringBuilder, string, StringBuilder>? append = null)
        {
            if (strings.Length == 0)
            {
                return;
            }
            
            builder.Append(prefix);
            for (var i = 0; i < strings.Length; i++)
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
                if (i != strings.Length - 1)
                {
                    builder.Append(separator);
                }
            }
            builder.Append(postFix);
        }
    }
}
