using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public static class Utils
{
    extension(List<Either<IDataRow, QueryResult>> list)
    {
        public IAsyncResultSet<IDataRow> ToAsyncResultSet()
        {
            return new AsyncResultSet(list);
        }
    }

    private sealed class AsyncResultSet : IAsyncResultSet<IDataRow>
    {
        private List<Either<IDataRow, QueryResult>>.Enumerator _enumerator;

        public AsyncResultSet(List<Either<IDataRow, QueryResult>> list)
        {
            _enumerator = list.GetEnumerator();
        }

        public Either<IDataRow, QueryResult> Current => _enumerator.Current;
        
        public ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(_enumerator.MoveNext());
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}
