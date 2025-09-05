namespace Sqlx.Core.Result;

/// <summary>
/// Static constructor interface for converting an <see cref="IDataRow"/> to an instance of
/// <typeparamref name="TResult"/> but in most cases the result type will be the type itself.
/// </summary>
/// <typeparam name="TResult">Row decode result type</typeparam>
public interface IFromRow<out TResult> where TResult : notnull
{
    /// <summary>
    /// Convert an <see cref="IDataRow"/> instance into a new instance of
    /// <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="dataRow">database row to convert</param>
    /// <returns>a new instance of the result type</returns>
    /// <exception cref="Exceptions.ColumnDecodeError">if decoding a column value fails</exception>
    public static abstract TResult FromRow(IDataRow dataRow);
}
