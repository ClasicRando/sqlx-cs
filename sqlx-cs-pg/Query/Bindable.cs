using System.Runtime.CompilerServices;
using Sqlx.Core.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Extensions class for Postgres specific binding to an <see cref="IBindable"/> instance. These
/// extension methods are included when you include the Postgres module and assume your
/// <see cref="IBindable"/> instance is a <see cref="IPgBindable"/>.
/// </summary>
public static class Bindable
{
    extension(IPgBindable bindable)
    {
        /// <summary>
        /// Bind <typeparamref name="TType"/> parameter to query. This allows for any value that can
        /// be encoded using the type definition of <typeparamref name="TType"/> to be bound.
        /// </summary>
        /// <param name="value">Value to bind</param>
        /// <typeparam name="TType">DB Type definition to allow for encoding the value</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindPg<TType>(TType value)
            where TType : IPgDbType<TType>
        {
            bindable.BindPg<TType, TType>(value);
        }
    }
}
