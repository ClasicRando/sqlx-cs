using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Postgres specific <see cref="IBindable"/>. Requires a method that uses a
/// <see cref="IPgDbType{T}"/> to defer the type encoding to. Provides default implementations for
/// <see cref="IBindable"/> methods that defer to that method with the Postgres type definitions.
/// </summary>
internal interface IPgBindable : IBindable
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

internal static class PgBindable
{
    extension(IPgBindable bindable)
    {
        /// <summary>
        /// Bind a nullable value type. This defers to <see cref="IBindable.BindNull"/> when the
        /// value is null and otherwise calls <see cref="IPgBindable.BindPg"/>.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <typeparam name="TType">Type used to encode the value</typeparam>
        public void BindPgNullableStruct<TValue, TType>(TValue? value)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            if (!value.HasValue)
            {
                bindable.BindNull<TType>();
            }
            else
            {
                bindable.BindPg<TValue, TType>(value.Value);
            }
        }
        
        /// <summary>
        /// Bind a nullable ref type. This defers to <see cref="IBindable.BindNull"/> when the value
        /// is null and otherwise calls <see cref="IPgBindable.BindPg"/>.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <typeparam name="TType">Type used to encode the value</typeparam>
        public void BindPgNullableClass<TValue, TType>(TValue? value)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            if (value is null)
            {
                bindable.BindNull<TType>();
            }
            else
            {
                bindable.BindPg<TValue, TType>(value);
            }
        }
    }
}
