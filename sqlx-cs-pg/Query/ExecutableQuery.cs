using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

public static class ExecutableQuery
{
    extension(IExecutableQuery executableQuery)
    {
        /// <summary>
        /// Execute the query and extract the first row's first column as the desired type
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TDecode">Type decoder for <typeparamref name="TValue"/></typeparam>
        /// <typeparam name="TValue">Final type to decode a scalar value for</typeparam>
        /// <returns>The first row's first column decoded as the desired value type</returns>
        /// <exception cref="Exception">If the query or column decoding fails</exception>
        public async Task<TValue> ExecuteScalar<TDecode, TValue>(
            CancellationToken cancellationToken = default)
            where TDecode : IPgDbType<TValue>
            where TValue : notnull
        {
            var flow = await executableQuery.Execute(cancellationToken);
            await foreach (var item in flow.WithCancellation(cancellationToken))
            {
                if (item is Either<IDataRow, QueryResult>.Left left)
                {
                    return PgException.CheckIfIs<IDataRow, PgDataRow>(left.Value)
                        .DecodeNotNull<TValue, TDecode>(0);
                }
            }

            throw new PgException("Query returned no rows");
        }

        /// <summary>
        /// Execute the query and extract the first row's first column as the desired type
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TType">Type that can decode itself from a row column</typeparam>
        /// <returns>The first row's first column decoded as the desired type</returns>
        /// <exception cref="Exception">If the query or column decoding fails</exception>
        public Task<TType> ExecuteScalarPg<TType>(CancellationToken cancellationToken = default)
            where TType : IPgDbType<TType>
        {
            return executableQuery.ExecuteScalar<TType, TType>(cancellationToken);
        }

        /// <summary>
        /// Execute the query and extract the first row's first column as a JSON of the desired type
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <param name="jsonTypeInfo">Optional JSON type deserialization info</param>
        /// <typeparam name="TValue"></typeparam>
        /// <returns>The first row's first column decoded as JSON of the desired type</returns>
        /// <exception cref="Exception">
        /// If the query, column decoding or deserialization fails
        /// </exception>
        public async Task<TValue> ExecuteScalarJson<TValue>(
            JsonTypeInfo<TValue>? jsonTypeInfo = null,
            CancellationToken cancellationToken = default)
            where TValue : notnull
        {
            var flow = await executableQuery.Execute(cancellationToken);
            await foreach (var item in flow.WithCancellation(cancellationToken))
            {
                if (item is Either<IDataRow, QueryResult>.Left left)
                {
                    return PgException.CheckIfIs<IDataRow, PgDataRow>(left.Value)
                        .GetJsonNotNull(0, jsonTypeInfo);
                }
            }

            throw new PgException("Query returned no rows");
        }
    }
}
