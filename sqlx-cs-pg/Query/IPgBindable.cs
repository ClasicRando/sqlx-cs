using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Postgres specific <see cref="IBindable"/>. Requires a method that uses a
/// <see cref="IPgDbType{T}"/> to defer the type encoding to. Provides default implementations for
/// <see cref="IBindable"/> methods that defer to that method with the Postgres type definitions.
/// </summary>
public interface IPgBindable : IBindable
{
    /// <summary>
    /// Bind the value using the Postgres definition type <typeparamref name="TType"/>. In cases
    /// where the value is a non-core type then the types parameters will be the same type since
    /// those types would implement <see cref="IPgDbType{T}"/> on itself rather than a proxy type.
    /// </summary>
    /// <param name="value">Value to bind</param>
    /// <typeparam name="TValue">Type of the value</typeparam>
    /// <typeparam name="TType">Type used to encode the value</typeparam>
    void BindPg<TValue, TType>(TValue value)
        where TType : IPgDbType<TValue>
        where TValue : notnull;

    /// <summary>
    /// Bind a byte[] value. This maps to the <c>VARBINARY</c>/<c>BLOB</c> type although database
    /// implementations might have custom types (e.g. Postgres has <c>BYTEA</c>)
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;byte&gt; value</param>
    void Bind(in ReadOnlySpan<byte> value);

    /// <summary>
    /// Bind a string value. This maps to the <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type although
    /// database implementations might have custom character types (e.g. Postgres has
    /// <c>CITEXT</c>).
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;char&gt; value</param>
    void Bind(in ReadOnlySpan<char> value);

    void Bind<T>(T value) => throw new NotImplementedException();
}
