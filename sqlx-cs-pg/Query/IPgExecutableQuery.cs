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
    IReadOnlyList<PgTypeInfo> PgTypes { get; }
    
    /// <summary>
    /// Encoded bytes for each parameter. This should only be inspected by
    /// <see cref="Sqlx.Postgres.Stream.PgStream"/>.
    /// </summary>
    ReadOnlySpan<byte> EncodedParameters { get; }
}
