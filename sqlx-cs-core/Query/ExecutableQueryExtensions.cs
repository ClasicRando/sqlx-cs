using System.Runtime.CompilerServices;
using Sqlx.Core.Result;

namespace Sqlx.Core.Query;

public static class ExecutableQueryExtensions
{
    public static async Task<long> ExecuteNonQuery(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken)
    {
        long count = 0;
        var results = executableQuery.Execute(cancellationToken);
        await foreach (var result in results)
        {
            if (result is { Right: not null })
            {
                count += result.Right.RowsAffected;
            }
        }

        return count;
    }

    public static async IAsyncEnumerable<TRow> Fetch<TRow>(
        this IExecutableQuery executableQuery,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where TRow : IFromRow<TRow>
    {
        var results = executableQuery.Execute(cancellationToken)
            .ConfigureAwait(false);
        await foreach (var result in results)
        {
            switch (result)
            {
                case { Right: not null }:
                    yield break;
                case { Left: not null }:
                    yield return TRow.Decode(result.Left);
                    break;
            }
        }
    }

    public static ValueTask<List<TRow>> FetchAll<TRow>(
        this IExecutableQuery executableQuery,
        CancellationToken cancellationToken)
        where TRow : IFromRow<TRow>
    {
        return Fetch<TRow>(executableQuery, cancellationToken).ToListAsync(cancellationToken);
    }
}
