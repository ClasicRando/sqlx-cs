using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Query;

public class QueryBatchExhausted : SqlxException
{
    public QueryBatchExhausted() : base(
        "Attempted to extract a result from a query batch result that was already exhausted")
    {
    }
}
