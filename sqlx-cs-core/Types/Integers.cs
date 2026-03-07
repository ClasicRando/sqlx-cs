using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

public static class Integers
{
    [DoesNotReturn]
    public static void ThrowColumnDecodeException<TType, TMetadata>(TMetadata columnMetadata)
        where TType: unmanaged
        where TMetadata : IColumnMetadata
    {
        throw ColumnDecodeException.Create<TType, TMetadata>(
            columnMetadata,
            $"Value is outside of valid {typeof(TType).Name}");
    }

    public static bool IsValidUInt(long value)
    {
        return value is >= uint.MinValue and <= uint.MaxValue;
    }

    public static bool IsValidInt(long value)
    {
        return value is >= int.MinValue and <= int.MaxValue;
    }

    public static bool IsValidShort(long value)
    {
        return value is >= short.MinValue and <= short.MaxValue;
    }

    public static bool IsValidByte(long value)
    {
        return value is >= byte.MinValue and <= byte.MaxValue;
    }
}
