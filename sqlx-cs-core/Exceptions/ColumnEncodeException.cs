using System.Runtime.CompilerServices;

namespace Sqlx.Core.Exceptions;

public class ColumnEncodeException(
    int dataTypeId,
    Type encodeType,
    string reason = "",
    Exception? cause = null) : SqlxException(
    $"Could not encode value into desired type. Actual type ID: {dataTypeId}. " +
    $"Input: {encodeType}" +
    $"{(string.IsNullOrWhiteSpace(reason) ? string.Empty : $"\n{reason}")}",
    cause)
{
    /// <summary>
    /// Create a new <see cref="ColumnEncodeException"/> using the supplied data
    /// </summary>
    /// <param name="dataTypeId">Type ID of the desired encoding</param>
    /// <param name="reason">optional reason for the encoding failure</param>
    /// <param name="cause">optional cause for the encoding failure</param>
    /// <typeparam name="T">CLR decoding type</typeparam>
    /// <returns>exception instance that can be thrown</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColumnEncodeException Create<T>(
        int dataTypeId,
        string reason = "",
        Exception? cause = null) where T : notnull
    {
        return new ColumnEncodeException(dataTypeId, typeof(T), reason, cause);
    }
}
