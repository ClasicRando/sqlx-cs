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
        public ValueTask<TValue> ExecuteScalar<TValue, TType>(
            CancellationToken cancellationToken = default)
            where TType : IPgDbType<TValue>
            where TValue : notnull
        {
            return executableQuery.ExecuteAsync(cancellationToken)
                .OfType<Either<IPgDataRow, QueryResult>.Left>()
                .Select(item => item.Value.GetPgNotNull<TValue, TType>(0))
                .FirstAsync(cancellationToken);
        }

        /// <summary>
        /// Execute the query and extract the first row's first column as the desired type
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <typeparam name="TType">Type that can decode itself from a row column</typeparam>
        /// <returns>The first row's first column decoded as the desired type</returns>
        /// <exception cref="Exception">If the query or column decoding fails</exception>
        public ValueTask<TType> ExecuteScalar<TType>(CancellationToken cancellationToken = default)
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
            TValue? result = await executableQuery.ExecuteAsync(cancellationToken)
                .OfType<Either<IPgDataRow, QueryResult>.Left>()
                .Select(item => item.Value.GetJsonNotNull(0, jsonTypeInfo))
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            return result ?? throw new PgException("Query returned no rows");
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchAsync{TDataRow,TRow}"/>>
        public IAsyncEnumerable<TRow> FetchAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchAllAsync{TDataRow,TRow}"/>>
        public ValueTask<List<TRow>> FetchAllAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchAllAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchFirstAsync{TDataRow,TRow}"/>>
        public Task<TRow> FetchFirstAsync<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchFirstAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchFirstOrDefaultAsync{TDataRow,TRow}"/>>
        public Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchFirstOrDefaultAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchSingleAsync{TDataRow,TRow}"/>>
        public Task<TRow> FetchSingleAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchSingleAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchSingleOrDefaultAsync{TDataRow,TRow}"/>>
        public Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchSingleOrDefaultAsync<IPgDataRow, TRow>(cancellationToken);
        }
    }
}
