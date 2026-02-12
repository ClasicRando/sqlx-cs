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
                    case { IsRight: true }:
                        result.Add((rowBuffer, item.Right));
                        rowBuffer = [];
                        break;
                    case { IsLeft: true }:
                        rowBuffer.Add(item.Left);
                        break;
                }
            }

            return result;
        }
    }
}
