using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Result;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres;

public static class TestExtensions
{
    public static async Task<List<(List<IPgDataRow>, QueryResult)>> CollectResults(
        this IAsyncEnumerable<Either<IPgDataRow, QueryResult>> flow)
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

    extension(IPgExecutableQuery executableQuery)
    {
        public Task<TValue> ExecuteScalar<TDecode, TValue>()
            where TDecode : IPgDbType<TValue>
            where TValue : notnull
        {
            return executableQuery.ExecuteScalar<TValue, TDecode>(
                TestContext.Current.CancellationToken);
        }

        public Task<TType> ExecuteScalarPg<TType>()
            where TType : IPgDbType<TType>
        {
            return executableQuery.ExecuteScalar<TType, TType>(
                TestContext.Current.CancellationToken);
        }

        public Task<TValue> ExecuteScalarJson<TValue>(JsonTypeInfo<TValue>? jsonTypeInfo = null)
            where TValue : notnull
        {
            return executableQuery.ExecuteScalarJson(
                jsonTypeInfo,
                TestContext.Current.CancellationToken);
        }
    }
}
