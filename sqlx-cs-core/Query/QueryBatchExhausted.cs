using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Query;

[SuppressMessage(
    "Design",
    "CA1032:Implement standard exception constructors",
    Justification =
        "This is a very specific exception that should not be used like a general exception")]
public class QueryBatchExhausted : SqlxException
{
    public QueryBatchExhausted() : base(
        "Attempted to extract a result from a query batch result that was already exhausted")
    {
    }
}
