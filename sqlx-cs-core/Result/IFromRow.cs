using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Result;

/// <summary>
/// Static constructor interface for converting an <see cref="IDataRow"/> to an instance of
/// <typeparamref name="TResult"/> but in most cases the result type will be the type itself.
/// </summary>
/// <typeparam name="TDataRow">Data row type that can be deserialized</typeparam>
/// <typeparam name="TResult">Row decode result type</typeparam>
public interface IFromRow<in TDataRow, out TResult>
    where TDataRow : IDataRow
    where TResult : notnull
{
    /// <summary>
    /// Convert a <see cref="TDataRow"/> instance into a new instance of
    /// <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="dataRow">database row to convert</param>
    /// <returns>a new instance of the result type</returns>
    /// <exception cref="ColumnDecodeException">if decoding a column value fails</exception>
    static abstract TResult FromRow(TDataRow dataRow);
}
