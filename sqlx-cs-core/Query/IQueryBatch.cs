using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public interface IQueryBatch
{
    public IQuery CreateQuery(string sql);

    public IAsyncEnumerable<Either<IDataRow, QueryResult>> ExecuteBatch();
}
