namespace Sqlx.Core.Result;

public interface IAsyncResultSet<TDataRow> : IDisposable where TDataRow : IDataRow
{
    Either<TDataRow, QueryResult> Current { get; }

    ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken = default);
}
