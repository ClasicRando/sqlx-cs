using Sqlx.Core.Result;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Result;

/// <summary>
/// Extension interface of <see cref="IDataRow"/> for Postgres specific column decoding
/// </summary>
internal interface IPgDataRow : IDataRow
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

internal static class PgDataRowExtensions
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
        public TValue GetPgNotNull<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : notnull
        {
            return pgDataRow.GetPgNotNull<TValue, TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to reference types. For value based types see
        /// <see cref="GetPgVal{TValue,TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        public TValue? GetPgRef<TValue, TType>(int index)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            return pgDataRow.IsNull(index) ? null : pgDataRow.GetPgNotNull<TValue, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to reference types. For value based types see
        /// <see cref="GetPgVal{TValue,TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        public TValue? GetPgRef<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : class
        {
            return pgDataRow.GetPgRef<TValue, TType>(pgDataRow.IndexOf(name));
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to value types. For reference based types see
        /// <see cref="GetPgRef{TValue,TType}(IPgDataRow, int)"/>
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        public TValue? GetPgVal<TValue, TType>(int index)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            return pgDataRow.IsNull(index) ? null : pgDataRow.GetPgNotNull<TValue, TType>(index);
        }
        
        /// <summary>
        /// Extract a value of type <typeparamref name="TValue"/> from this row using the type
        /// definition of <typeparamref name="TType"/>. Allows for null return values and is
        /// specific to value types. For reference based types see
        /// <see cref="GetPgRef{TValue,TType}(IPgDataRow, string)"/>
        /// </summary>
        /// <param name="name">column name to extract</param>
        /// <typeparam name="TValue">Return type</typeparam>
        /// <typeparam name="TType">Type definition</typeparam>
        /// <returns>
        /// <typeparamref name="TValue"/> value at the specified column or null if the DB value is
        /// null
        /// </returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">
        /// if the column cannot be decoded as <typeparamref name="TType"/>
        /// </exception>
        public TValue? GetPgVal<TValue, TType>(string name)
            where TType : IPgDbType<TValue>
            where TValue : struct
        {
            return pgDataRow.GetPgVal<TValue, TType>(pgDataRow.IndexOf(name));
        }
    }
}
