using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

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

    public static async Task<TValue> ExecuteScalar<TDecode, TValue>(
        this IExecutableQuery executableQuery)
        where TDecode : IPgDbType<TValue>
        where TValue : notnull
    {
        var flow = await executableQuery.Execute(TestContext.Current.CancellationToken);
        await foreach (var item in flow.WithCancellation(TestContext.Current.CancellationToken))
        {
            if (item is Either<IDataRow, QueryResult>.Left left)
            {
                return PgException.CheckIfIs<IDataRow, PgDataRow>(left.Value)
                    .DecodeNotNull<TValue, TDecode>(0);
            }
        }

        throw new Exception("Query returned no rows");
    }

    public static Task<TType> ExecuteScalarPg<TType>(this IExecutableQuery executableQuery)
        where TType : IPgDbType<TType>
    {
        return ExecuteScalar<TType, TType>(executableQuery);
    }

    public static async Task<TValue> ExecuteScalarJson<TValue>(
        this IExecutableQuery executableQuery,
        JsonTypeInfo<TValue>? jsonTypeInfo = null)
        where TValue : notnull
    {
        var flow = await executableQuery.Execute(TestContext.Current.CancellationToken);
        await foreach (var item in flow.WithCancellation(TestContext.Current.CancellationToken))
        {
            if (item is Either<IDataRow, QueryResult>.Left left)
            {
                return PgException.CheckIfIs<IDataRow, PgDataRow>(left.Value)
                    .GetJsonNotNull(0, jsonTypeInfo);
            }
        }

        throw new Exception("Query returned no rows");
    }
}
