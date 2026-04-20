using Sqlx.Core.Query;
using Sqlx.Postgres.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// <see cref="IExecutableQuery{TDataRow}"/> for Postgres
/// </summary>
public interface IPgExecutableQuery : IExecutableQuery<IPgDataRow>, IPgBindable
{
    /// <summary>
    /// Number of parameters encoded into the query. Postgres caps that number to a short
    /// </summary>
    short ParameterCount { get; }

    /// <summary>
    /// Type info for each encoded parameter. Will be <see cref="PgTypeInfo.Unknown"/> when the
    /// parameter value is null.
    /// </summary>
    IReadOnlyList<PgTypeInfo> ParameterPgTypes { get; }

    /// <summary>
    /// Encoded bytes for each parameter. This should only be inspected by
    /// <see cref="Connector.PgConnector"/>.
    /// </summary>
    ReadOnlySpan<byte> EncodedParameters { get; }

    /*
     * This method below will never have implementation since it is intended to be used with the
     * source interceptor provided in 'sqlx-cs-pg-generator' to provide the actual method call at
     * build time. The interceptor will resolve the type and extract the value without boxing or
     * using dynamic dispatch.
     */
    
    /// <summary>
    /// <para>
    /// Execute this query and extract the first row's first column as the desired type. The value
    /// must NEVER be null.
    /// </para>
    /// <para>
    /// This method is intended to be used with the source interceptor provided in
    /// <c>sqlx-cs-pg-generator</c>. Without that dependency, this method always throws a
    /// <see cref="NotImplementedException"/>.
    /// </para>
    /// <para>
    /// Internally, this method will invoke <see cref="ExecutableQuery.ExecuteScalarPg"/> with the
    /// correct database type based upon <typeparamref name="T"/>.
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation</param>
    /// <typeparam name="T">Return type</typeparam>
    /// <returns>The first value returned from the query</returns>
    Task<T> ExecuteScalar<T>(CancellationToken cancellationToken = default) where T : notnull =>
        throw new NotImplementedException();
}
