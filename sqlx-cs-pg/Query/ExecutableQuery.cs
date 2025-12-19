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
    extension(IPgExecutableQuery executableQuery)
    {
        /// <summary>
        /// Execute the query and extract the first row's first column as the desired type
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TType">Type decoder for <typeparamref name="TValue"/></typeparam>
        /// <typeparam name="TValue">Final type to decode a scalar value for</typeparam>
        /// <returns>The first row's first column decoded as the desired value type</returns>
        /// <exception cref="Exception">If the query or column decoding fails</exception>
        public async Task<TValue> ExecuteScalar<TValue, TType>(
            CancellationToken cancellationToken = default)
            where TType : IPgDbType<TValue>
            where TValue : notnull
        {
            var flow = await executableQuery.Execute(cancellationToken);
            await foreach (var item in flow.WithCancellation(cancellationToken))
            {
                if (item is Either<IPgDataRow, QueryResult>.Left left)
                {
                    return PgException.CheckIfIs<IDataRow, IPgDataRow>(left.Value)
                        .GetPgNotNull<TValue, TType>(0);
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
                if (item is Either<IPgDataRow, QueryResult>.Left left)
                {
                    return PgException.CheckIfIs<IDataRow, PgDataRow>(left.Value)
                        .GetJsonNotNull(0, jsonTypeInfo);
                }
            }

            throw new PgException("Query returned no rows");
        }
        
        /// <inheritdoc cref="Core.Query.ExecutableQuery.ExecuteNonQuery"/>>
        public Task<long> ExecuteNonQuery(CancellationToken cancellationToken = default)
        {
            return executableQuery.ExecuteNonQuery<IPgDataRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.Fetch"/>>
        public IAsyncEnumerable<TRow> Fetch<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TRow>
        {
            return executableQuery.Fetch<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchAll"/>>
        public ValueTask<List<TRow>> FetchAll<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TRow>
        {
            return executableQuery.FetchAll<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchFirst"/>>
        public Task<TRow> FetchFirst<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TRow>
        {
            return executableQuery.FetchFirst<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchFirstOrDefault"/>>
        public Task<TRow?> FetchFirstOrDefault<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TRow>
        {
            return executableQuery.FetchFirstOrDefault<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchSingle"/>>
        public async Task<TRow> FetchSingle<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TRow>
        {
            return await executableQuery.FetchSingle<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchSingleOrDefault"/>>
        public Task<TRow?> FetchSingleOrDefault<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<TRow>
        {
            return executableQuery.FetchSingleOrDefault<IPgDataRow, TRow>(cancellationToken);
        }
    }
}
