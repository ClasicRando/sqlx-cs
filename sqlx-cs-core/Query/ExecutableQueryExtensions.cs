using System.Runtime.CompilerServices;
using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public static class ExecutableQueryExtensions
{
    public static async Task<long> ExecuteNonQuery(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
    {
        long count = 0;
        var results = executableQuery.Execute(cancellationToken);
        await foreach (var result in results)
        {
            if (result is Either<IDataRow, QueryResult>.Right right)
            {
                count += right.Value.RowsAffected;
            }
        }

        return count;
    }

    public static async IAsyncEnumerable<TRow> Fetch<TRow>(
        this IExecutableQuery executableQuery,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        var results = executableQuery.Execute(cancellationToken)
            .ConfigureAwait(false);
        await foreach (var result in results)
        {
            switch (result)
            {
                case Either<IDataRow, QueryResult>.Right:
                    yield break;
                case Either<IDataRow, QueryResult>.Left left:
                    yield return TRow.FromRow(left.Value);
                    break;
            }
        }
    }

    public static ValueTask<List<TRow>> FetchAll<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken = default)
        where TRow : IFromRow<TRow>
    {
        return Fetch<TRow>(executableQuery, cancellationToken).ToListAsync(cancellationToken);
    }
}
