using System.Runtime.CompilerServices;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

public static class DataRowExtensions
{
    extension(IPgDataRow pgDataRow)
    {
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>.
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns><typeparamref name="TValue"/> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column value is null or the column cannot be decoded as
        /// <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetPgNotNull<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : notnull
        {
            return pgDataRow.GetPgNotNull<TValue, TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns><typeparamref name="TType"/> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column value is null or the column cannot be decoded as
        /// <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType GetPgNotNull<TType>(int index)
            where TType : IPgDbType<TType>
        {
            return pgDataRow.GetPgNotNull<TType, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TType"/> from this row.
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns><typeparamref name="TType"/> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column value is null or the column cannot be decoded as
        /// <typeparamref name="TType"/>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TType GetPgNotNull<TType>(string name)
            where TType : IPgDbType<TType>
        {
            return pgDataRow.GetPgNotNull<TType, TType>(name);
        }
    }
}
