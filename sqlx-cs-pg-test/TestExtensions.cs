using Sqlx.Core.Result;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres;

public static class TestExtensions
{
    extension(Task<IAsyncResultSet<IPgDataRow>> flow)
    {
        public async Task<List<(List<IPgDataRow>, QueryResult)>> CollectResults()
        {
            List<(List<IPgDataRow>, QueryResult)> result = [];
            List<IPgDataRow> rowBuffer = [];
            using var resultSet = await flow;
            while (await resultSet.MoveNextAsync())
            {
                var item = resultSet.Current;
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
