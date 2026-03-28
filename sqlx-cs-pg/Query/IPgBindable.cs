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
}
