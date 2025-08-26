using System.Text;

namespace Sqlx.Core.Query;

public static class QueryUtils
{
    public static int QueryCount(ReadOnlySpan<char> sql)
    {
        var result = 0;
        var inQuote = false;
        var hasNonWhitespaceText = false;
        foreach (var c in sql)
        {
            switch (c)
            {
                case '\'':
                    inQuote = !inQuote;
                    hasNonWhitespaceText = true;
                    break;
                case ';':
                    if (!inQuote)
                    {
                        result++;
                        hasNonWhitespaceText = false;
                    }
                    break;
                default:
                    if (!char.IsWhiteSpace(c))
                    {
                        hasNonWhitespaceText = true;
                    }
                    break;
            }
        }

        if (sql[^1] is not ';' && hasNonWhitespaceText)
        {
            result++;
        }
        return result;
    }
    
    public static List<string> SplitQuery(string sql)
    {
        return !sql.Contains(';') ? [sql] : SplitQueryInner().ToList();

        IEnumerable<string> SplitQueryInner()
        {
            var builder = new StringBuilder();
            var inQuote = false;
            foreach (var c in sql)
            {
                switch (c)
                {
                    case '\'':
                        inQuote = !inQuote;
                        builder.Append(c);
                        break;
                    case ';':
                        if (inQuote)
                        {
                            builder.Append(c);
                        }
                        else
                        {
                            yield return builder.ToString();
                            builder.Clear();
                        }
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            if (builder.Length > 0)
            {
                yield return builder.ToString();
            }
        }
    }
}
