using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

/// <summary>
/// Wrapper type for <see cref="IQueryBatch{TBindable,TDataRow}"/> that allows for sequentially 
/// </summary>
/// <typeparam name="TBindable">Database specific bindable type</typeparam>
/// <typeparam name="TDataRow">Database specific row type</typeparam>
public sealed class QueryBatchResult<TBindable, TDataRow> : IDisposable
    where TBindable : IBindable
    where TDataRow : IDataRow
{
    private readonly IQueryBatch<TBindable, TDataRow> _queryBatch;
    private readonly IAsyncResultSet<TDataRow> _asyncResultSet;

    private QueryBatchResult(
        IQueryBatch<TBindable, TDataRow> queryBatch,
        IAsyncResultSet<TDataRow> asyncResultSet)
    {
        _queryBatch = queryBatch;
        _asyncResultSet = asyncResultSet;
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
        while (await _asyncResultSet.MoveNextAsync().ConfigureAwait(false))
        {
            var item = _asyncResultSet.Current;
            if (item.IsRight)
            {
                return result;
            }
            
            result.Add(TRow.FromRow(item.Left));
        }

        throw new QueryBatchExhausted();
    }

    public void Dispose()
    {
        _queryBatch.Dispose();
        _asyncResultSet.Dispose();
    }

    internal static async Task<QueryBatchResult<TBindable, TDataRow>> Create(
        IQueryBatch<TBindable, TDataRow> queryBatch,
        CancellationToken cancellationToken)
    {
        var resultStream = await queryBatch.ExecuteBatch(cancellationToken).ConfigureAwait(false);
        return new QueryBatchResult<TBindable, TDataRow>(queryBatch, resultStream);
    }
}
