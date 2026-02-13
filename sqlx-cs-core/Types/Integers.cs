using System.Diagnostics.CodeAnalysis;
using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

public static class Integers
{
    [DoesNotReturn]
    public static void ThrowColumnDecodeException<T>(IColumnMetadata columnMetadata)
        where T: unmanaged
    {
        throw ColumnDecodeException.Create<T>(
            columnMetadata,
            $"Value is outside of valid {typeof(T).Name}");
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
