using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sqlx.Core.Column;

namespace Sqlx.Core.Exceptions;

public class ColumnDecodeError(
    int dataTypeId,
    string typeName,
    string columnName,
    Type decodeType,
    string reason = "",
    Exception? cause = null) : SqlxException(
    $"Could not decode value into desired type. Actual type: {typeName}({dataTypeId}). " +
    $"Column: '{columnName}', Desired Output: {decodeType}" +
    $"{(string.IsNullOrWhiteSpace(reason) ? string.Empty : reason)}",
    cause)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColumnDecodeError Create<T>(
        IColumnMetadata metadata,
        string reason = "",
        Exception? cause = null) where T : notnull
    {
        return new ColumnDecodeError(
            metadata.DataType,
            metadata.DataType.ToString(),
            metadata.FieldName,
            typeof(T),
            reason,
            cause);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CheckOrThrow<T>(
        [DoesNotReturnIf(false)] bool check,
        IColumnMetadata metadata,
        Func<string> reason) where T : notnull
    {
        if (!check)
        {
            throw Create<T>(metadata, reason());
        }
    }
}
