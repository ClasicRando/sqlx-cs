using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Sqlx.Core.Column;

namespace Sqlx.Core.Exceptions;

/// <summary>
/// Special <see cref="SqlxException"/> for when column decoding fails. Requires
/// <see cref="IColumnMetadata"/> to create a detailed message.
/// </summary>
/// <param name="dataTypeId">database specific type ID</param>
/// <param name="typeName">name of the database type</param>
/// <param name="columnName">column name to be decoded</param>
/// <param name="decodeType">desired CLR type to decode a database value to</param>
/// <param name="reason">optional reason for the decoding failure</param>
/// <param name="cause">optional cause for the decoding failure</param>
public class ColumnDecodeException(
    uint dataTypeId,
    string typeName,
    string columnName,
    Type decodeType,
    string reason = "",
    Exception? cause = null) : SqlxException(
    $"Could not decode value into desired type. Actual type: {typeName}({dataTypeId}). " +
    $"Column: '{columnName}', Desired Output: {decodeType}" +
    $"{(string.IsNullOrWhiteSpace(reason) ? string.Empty : $"\n{reason}")}",
    cause)
{
    /// <summary>
    /// Create a new <see cref="ColumnDecodeException"/> using the supplied data
    /// </summary>
    /// <param name="metadata">column metadata instance to extract column type data</param>
    /// <param name="reason">optional reason for the decoding failure</param>
    /// <param name="cause">optional cause for the decoding failure</param>
    /// <typeparam name="TType">CLR decoding type</typeparam>
    /// <typeparam name="TMetadata">Metadata type</typeparam>
    /// <returns>exception instance that can be thrown</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColumnDecodeException Create<TType, TMetadata>(
        TMetadata metadata,
        string reason = "",
        Exception? cause = null)
        where TType : notnull
        where TMetadata : IColumnMetadata
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return new ColumnDecodeException(
            metadata.DataType,
            metadata.DataType.ToString(CultureInfo.InvariantCulture),
            metadata.FieldName,
            typeof(TType),
            reason,
            cause);
    }
}
