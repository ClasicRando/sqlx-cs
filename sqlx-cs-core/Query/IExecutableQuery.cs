using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public interface IExecutableQuery : IQuery
{
    public IAsyncEnumerable<Either<IDataRow, QueryResult>> Execute(CancellationToken cancellationToken);
}
