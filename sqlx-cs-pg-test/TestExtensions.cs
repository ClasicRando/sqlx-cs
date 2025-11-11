using Sqlx.Core;
using Sqlx.Core.Result;

namespace Sqlx.Postgres;

public static class TestExtensions
{
    public static async Task<List<(List<IDataRow>, QueryResult)>> CollectResults(
        this IAsyncEnumerable<Either<IDataRow, QueryResult>> flow)
    {
        List<(List<IDataRow>, QueryResult)> result = [];
        List<IDataRow> rowBuffer = [];
        await foreach (var item in flow)
        {
            switch (item)
            {
                case Either<IDataRow, QueryResult>.Right right:
                    result.Add((rowBuffer, right.Value));
                    rowBuffer = [];
                    break;
                case Either<IDataRow, QueryResult>.Left left:
                    rowBuffer.Add(left.Value);
                    break;
            }
        }

        return result;
    }
}
