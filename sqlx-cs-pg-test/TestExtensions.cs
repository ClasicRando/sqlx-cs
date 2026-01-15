using Sqlx.Core;
using Sqlx.Core.Result;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres;

public static class TestExtensions
{
    extension(IAsyncEnumerable<Either<IPgDataRow, QueryResult>> flow)
    {
        public async Task<List<(List<IPgDataRow>, QueryResult)>> CollectResults()
        {
            List<(List<IPgDataRow>, QueryResult)> result = [];
            List<IPgDataRow> rowBuffer = [];
            await foreach (var item in flow)
            {
                switch (item)
                {
                    case Either<IPgDataRow, QueryResult>.Right right:
                        result.Add((rowBuffer, right.Value));
                        rowBuffer = [];
                        break;
                    case Either<IPgDataRow, QueryResult>.Left left:
                        rowBuffer.Add(left.Value);
                        break;
                }
            }

            return result;
        }
    }
}
