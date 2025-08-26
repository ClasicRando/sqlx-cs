namespace Sqlx.Core.Result;

public record QueryResult(long RowsAffected, string Message)
{
    public QueryResult Merge(QueryResult other)
    {
        return new QueryResult(
            RowsAffected + other.RowsAffected,
            $"{Message},{other.Message}".Trim(','));
    }
}
