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

    /*
     * The 2 methods below will never have implementation since they are intended to be used with
     * the source interceptor provided in 'sqlx-cs-pg-generator' to provide the actual method call
     * at build time. The interceptor will resolve the type and extract the value without boxing or
     * using dynamic dispatch.
     */

    /// <summary>
    /// <para>
    /// Method to extract the desired type from the field specified as a zero-based index.
    /// </para>
    /// <para>
    /// This method is intended to be used with the source interceptor provided in
    /// <c>sqlx-cs-pg-generator</c>. Without that dependency, this method always throws a
    /// <see cref="NotImplementedException"/>.
    /// </para>
    /// <para>
    /// Internally, this method will invoke <see cref="GetPgNotNull"/> with the correct database
    /// type based upon <typeparamref name="T"/>. If the type parameter is nullable, then the field
    /// will first be checked for a null value.
    /// </para>
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <typeparam name="T">Return type</typeparam>
    /// <returns><typeparamref name="T"/> value at the specified column</returns>
    T GetField<T>(int index) => throw new NotImplementedException();

    /// <summary>
    /// <para>
    /// Method to extract the desired type from the field specified as a result set field name.
    /// </para>
    /// <para>
    /// This method is intended to be used with the source interceptor provided in
    /// <c>sqlx-cs-pg-generator</c>. Without that dependency, this method always throws a
    /// <see cref="NotImplementedException"/>.
    /// </para>
    /// <para>
    /// Internally, this method will invoke <see cref="GetPgNotNull"/> with the correct database
    /// type based upon <typeparamref name="T"/>. If the type parameter is nullable, then the field
    /// will first be checked for a null value.
    /// </para>
    /// </summary>
    /// <param name="name">name of the column to extract</param>
    /// <typeparam name="T">Return type</typeparam>
    /// <returns><typeparamref name="T"/> value at the specified column</returns>
    T GetField<T>(string name) => throw new NotImplementedException();
}
