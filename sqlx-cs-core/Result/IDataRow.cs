using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Column;

namespace Sqlx.Core.Result;

/// <summary>
/// Database query result row that allows for decoding columns into specific types. This interface
/// just defines extraction of standard SQL types and database specific types can be found as
/// extension methods on this interface. Since nullability can be annotated on return types, there
/// are 2 methods per type where the simple method allows for extracting database nulls as
/// <c>null</c> and the other method defines a non-null result where database nulls initiate a
/// thrown exception.
/// </summary>
public interface IDataRow
{
    /// <summary>
    /// Total number of columns for this row
    /// </summary>
    int ColumnCount { get; }

    /// <param name="name">Column name to check</param>
    /// <returns>
    /// The 0-based index of the column name specified or -1 if the name does not exist
    /// </returns>
    int IndexOf(string name);

    /// <summary>
    /// Obtain the metadata for the specified column index
    /// </summary>
    /// <param name="index">0-based index of the column to check</param>
    /// <returns>Metadata for the specified column</returns>
    IColumnMetadata GetColumnMetadata(int index);

    /// <param name="index">0-based index of the column to check</param>
    /// <returns>True if the value found at the index is a DB null</returns>
    bool IsNull(int index);

    /// <summary>
    /// Extract a not-null <typeparamref name="T"/> value after deserializing the inner value
    /// as JSON. Some databases have a JSON specific field type but other database drivers will
    /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
    /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
    /// </summary>
    /// <param name="index">0-based index of the column to extract</param>
    /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
    /// <returns><c>T</c> value at the specified column</returns>
    /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
    T GetJsonNotNull<T>(int index, JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull;
}

public static class DataRowExtensions
{
    extension(IDataRow dataRow)
    {
        /// <summary>
        /// Obtain the metadata for the specified column by name
        /// </summary>
        /// <param name="name">name of the column to check</param>
        /// <returns>Metadata for the specified column</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IColumnMetadata GetColumnMetadata(string name)
        {
            return dataRow.GetColumnMetadata(dataRow.IndexOf(name));
        }

        /// <summary>
        /// Extract a possibly null <typeparamref name="T"/> value after deserializing the inner value
        /// as JSON. Some databases have a JSON specific field type but other database drivers will
        /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
        /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
        /// </summary>
        /// <param name="index">0-based index of the column to extract</param>
        /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
        /// <returns>
        /// <c>T</c> value at the specified column or default if the DB value was null
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetJson<T>(
            int index,
            JsonTypeInfo<T>? jsonTypeInfo = null)
            where T : notnull
        {
            return dataRow.IsNull(index) ? default : dataRow.GetJsonNotNull(index, jsonTypeInfo);
        }

        /// <summary>
        /// Extract a possibly null <typeparamref name="T"/> value after deserializing the inner value
        /// as JSON. Some databases have a JSON specific field type but other database drivers will
        /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
        /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
        /// <returns><c>T</c> value at the specified column or null if the DB value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetJson<T>(
            string name,
            JsonTypeInfo<T>? jsonTypeInfo = null)
            where T : notnull
        {
            return dataRow.GetJson(dataRow.IndexOf(name), jsonTypeInfo);
        }

        /// <summary>
        /// Extract a not-null <typeparamref name="T"/> value after deserializing the inner value
        /// as JSON. Some databases have a JSON specific field type but other database drivers will
        /// treat string or byte[] fields as JSON compatible. When using this method, it's recommended
        /// to supply the <see cref="JsonTypeInfo{T}"/> parameter to aid deserialization.
        /// </summary>
        /// <param name="name">name of the column to extract</param>
        /// <param name="jsonTypeInfo">optional JSON source generated deserialization metadata</param>
        /// <returns><c>T</c> value at the specified column</returns>
        /// <exception cref="Sqlx.Core.Exceptions.SqlxException">if the column value is null</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetJsonNotNull<T>(
            string name,
            JsonTypeInfo<T>? jsonTypeInfo = null) where T : notnull
        {
            return dataRow.GetJsonNotNull(dataRow.IndexOf(name), jsonTypeInfo);
        }
    }
}
