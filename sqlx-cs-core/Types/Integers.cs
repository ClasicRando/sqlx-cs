using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

public static class Integers
{
    public static int ValidateInt(long value, IColumnMetadata columnMetadata)
    {
        ColumnDecodeError.CheckOrThrow<int>(
            value is >= int.MinValue and <= int.MaxValue,
            columnMetadata,
            "Valid is outside of valid int");
        return (int)value;

    }
    
    public static short ValidateShort(long value, IColumnMetadata columnMetadata)
    {
        ColumnDecodeError.CheckOrThrow<short>(
            value is >= short.MinValue and <= short.MaxValue,
            columnMetadata,
            "Valid is outside of valid short");
        return (short)value;

    }
    
    public static byte ValidateByte(long value, IColumnMetadata columnMetadata)
    {
        ColumnDecodeError.CheckOrThrow<byte>(
            value is >= byte.MinValue and <= byte.MaxValue,
            columnMetadata,
            "Valid is outside of valid byte");
        return (byte)value;

    }
}
