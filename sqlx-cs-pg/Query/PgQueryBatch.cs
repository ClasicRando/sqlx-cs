using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Postgres.Query;

public sealed class PgQueryBatch : IQueryBatch
{
    public bool WrapBatchInTransaction { get; set; }
    
    public IQuery CreateQuery(string sql)
    {
        throw new NotImplementedException();
    }

    public Task<IAsyncEnumerable<Either<IDataRow, QueryResult>>> ExecuteBatch(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
