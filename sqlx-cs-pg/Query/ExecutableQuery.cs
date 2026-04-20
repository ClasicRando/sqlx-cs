using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Query;
using Sqlx.Core.Result;
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
        public async Task<TValue> ExecuteScalarPg<TValue, TType>(
            CancellationToken cancellationToken = default)
            where TType : IPgDbType<TValue>
            where TValue : notnull
        {
            using var resultSet = await executableQuery.ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!await resultSet.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Scalar queries must return at least 1 row");
            }

            var current = resultSet.Current;
            return current.IsLeft
                ? current.Left.GetPgNotNull<TValue, TType>(0)
                : throw new InvalidOperationException("Scalar query cannot be non-query");
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
            using var resultSet = await executableQuery.ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!await resultSet.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Scalar queries must return at least 1 row");
            }

            var current = resultSet.Current;
            return current.IsLeft
                ? current.Left.GetJsonNotNull(0, jsonTypeInfo)
                : throw new InvalidOperationException("Scalar query cannot be non-query");
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchAsync{TDataRow,TRow}"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerable<TRow> FetchAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchAllAsync{TDataRow,TRow}"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<List<TRow>> FetchAllAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchAllAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchFirstAsync{TDataRow,TRow}"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchFirstAsync<TRow>(CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchFirstAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchFirstOrDefaultAsync{TDataRow,TRow}"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchFirstOrDefaultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchFirstOrDefaultAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchSingleAsync{TDataRow,TRow}"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow> FetchSingleAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchSingleAsync<IPgDataRow, TRow>(cancellationToken);
        }

        /// <inheritdoc cref="Core.Query.ExecutableQuery.FetchSingleOrDefaultAsync{TDataRow,TRow}"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRow?> FetchSingleOrDefaultAsync<TRow>(
            CancellationToken cancellationToken = default)
            where TRow : IFromRow<IPgDataRow, TRow>
        {
            return executableQuery.FetchSingleOrDefaultAsync<IPgDataRow, TRow>(cancellationToken);
        }
    }
}
