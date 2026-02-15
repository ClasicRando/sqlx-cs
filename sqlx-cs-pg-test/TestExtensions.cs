using Sqlx.Core.Result;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres;

public static class TestExtensions
{
    extension(IAsyncResultSet<IPgDataRow> resultSet)
    {
        public async Task<List<(List<T>, QueryResult)>> CollectResults<T>(Func<IPgDataRow, T> rowExtractor)
        {
            List<(List<T>, QueryResult)> result = [];
            List<T> rowBuffer = [];
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
                        rowBuffer.Add(rowExtractor(item.Left));
                        break;
                }
            }

            return result;
        }
    }
}
