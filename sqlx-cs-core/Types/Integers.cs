using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

public static class Integers
{
    [StackTraceHidden]
    [DoesNotReturn]
    public static void ThrowColumnDecodeException<TType, TMetadata>(TMetadata columnMetadata)
        where TType: unmanaged
        where TMetadata : IColumnMetadata
    {
        throw ColumnDecodeException.Create<TType, TMetadata>(
            columnMetadata,
            $"Value is outside of valid {typeof(TType).Name}");
    }

    /// <summary>
    /// Check to see if an extract <see cref="long"/> is within the valid range for a
    /// <see cref="uint"/>
    /// </summary>
    /// <param name="value">64-bit integer</param>
    /// <returns>True if the long value is a valid uint</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidUInt(long value)
    {
        return value is >= uint.MinValue and <= uint.MaxValue;
    }

    /// <summary>
    /// Check to see if an extract <see cref="long"/> is within the valid range for an
    /// <see cref="int"/>
    /// </summary>
    /// <param name="value">64-bit integer</param>
    /// <returns>True if the long value is a valid int</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidInt(long value)
    {
        return value is >= int.MinValue and <= int.MaxValue;
    }

    /// <summary>
    /// Check to see if an extract <see cref="long"/> is within the valid range for a
    /// <see cref="short"/>
    /// </summary>
    /// <param name="value">64-bit integer</param>
    /// <returns>True if the long value is a valid short</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidShort(long value)
    {
        return value is >= short.MinValue and <= short.MaxValue;
    }

    /// <summary>
    /// Check to see if an extract <see cref="long"/> is within the valid range for a
    /// <see cref="byte"/>
    /// </summary>
    /// <param name="value">64-bit integer</param>
    /// <returns>True if the long value is a valid byte</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidByte(long value)
    {
        return value is >= byte.MinValue and <= byte.MaxValue;
    }
}
