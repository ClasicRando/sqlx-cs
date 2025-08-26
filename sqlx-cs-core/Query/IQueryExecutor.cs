using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public interface IQueryExecutor
{
    public IAsyncEnumerable<Either<IDataRow, QueryResult>> ExecuteQuery(
        IQuery query,
        CancellationToken cancellationToken);
}
