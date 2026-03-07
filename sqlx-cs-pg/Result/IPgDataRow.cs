using Sqlx.Core.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

/// <summary>
/// Extension interface of <see cref="IDataRow"/> for Postgres specific column decoding
/// </summary>
public interface IPgDataRow : IDataRow, IDisposable
{
    /// <summary>
    /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
    /// definition of <typeparamref name="TType"/>.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <typeparam name="TValue">Return type</typeparam>
    /// <typeparam name="TType">Type definition</typeparam>
    /// <returns><typeparamref name="TValue"/> value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
    /// if the column value is null or the column cannot be decoded as <typeparamref name="TType"/>
    /// </exception>
    TValue GetPgNotNull<TValue, TType>(int index)
        where TType : IPgDbType<TValue>
        where TValue : notnull;
}
