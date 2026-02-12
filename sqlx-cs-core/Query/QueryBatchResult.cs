using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Wrapper type for <see cref="IQueryBatch{TBindable,TDataRow}"/> that allows for sequentially 
/// </summary>
/// <typeparam name="TBindable">Database specific bindable type</typeparam>
/// <typeparam name="TDataRow">Database specific row type</typeparam>
public sealed class QueryBatchResult<TBindable, TDataRow> : IAsyncDisposable
    where TBindable : IBindable
    where TDataRow : IDataRow
{
    private readonly IQueryBatch<TBindable, TDataRow> _queryBatch;
    private readonly IAsyncEnumerator<Either<TDataRow, QueryResult>> _resultStreamEnumerator;

    private QueryBatchResult(
        IQueryBatch<TBindable, TDataRow> queryBatch,
        IAsyncEnumerator<Either<TDataRow, QueryResult>> resultStreamEnumerator)
    {
        _queryBatch = queryBatch;
        _resultStreamEnumerator = resultStreamEnumerator;
    }

    /// <summary>
    /// Enumerate the result stream of this query batch 
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    /// <returns></returns>
    /// <exception cref="QueryBatchExhausted"></exception>
    public async Task<List<TRow>> ExtractNextResultAsync<TRow>()
        where TRow : IFromRow<TDataRow, TRow>
    {
        List<TRow> result = [];
        while (await _resultStreamEnumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var item = _resultStreamEnumerator.Current;
            if (item.IsRight)
            {
                return result;
            }
            
            result.Add(TRow.FromRow(item.Left));
        }

        throw new QueryBatchExhausted();
    }

    public async ValueTask DisposeAsync()
    {
        _queryBatch.Dispose();
        await _resultStreamEnumerator.DisposeAsync().ConfigureAwait(false);
    }

    internal static QueryBatchResult<TBindable, TDataRow> Create(
        IQueryBatch<TBindable, TDataRow> queryBatch,
        CancellationToken cancellationToken)
    {
        var resultStream = queryBatch.ExecuteBatch(cancellationToken);
        var resultStreamEnumerator = resultStream.GetAsyncEnumerator(cancellationToken);
        return new QueryBatchResult<TBindable, TDataRow>(queryBatch, resultStreamEnumerator);
    }
}
