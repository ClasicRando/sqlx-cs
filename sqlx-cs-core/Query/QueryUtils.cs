namespace Sqlx.Core.Query;

public static class QueryUtils
{
    /// <summary>
    /// Search through the provided SQL block to find the total number of queries within. Queries
    /// are expected to be separated by ';' so only SQL dialects with required separators work for
    /// this method (e.g. T-SQL blocks might not work because semicolons are not required to be
    /// valid SQL). 
    /// </summary>
    /// <param name="sql">SQL block to analyze</param>
    /// <returns>total number of SQL queries within the block</returns>
    public static int QueryCount(ReadOnlySpan<char> sql)
    {
        sql = sql.Trim();
        var result = 0;
        var inQuote = false;
        foreach (var c in sql)
        {
            switch (c)
            {
                case '\'':
                    inQuote = !inQuote;
                    break;
                case ';':
                    if (!inQuote)
                    {
                        result++;
                    }
                    break;
            }
        }

        if (sql[^1] is not ';')
        {
            result++;
        }
        return result;
    }
}
