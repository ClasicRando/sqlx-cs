using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

public static class Integers
{
    public static int ValidateInt(long value, IColumnMetadata columnMetadata)
    {
        if (value is < int.MinValue or > int.MaxValue)
        {
            throw ColumnDecodeError.Create<int>(
                columnMetadata,
                "Valid is outside of valid int");
        }
        return (int)value;

    }
    
    public static short ValidateShort(long value, IColumnMetadata columnMetadata)
    {
        if (value is < int.MinValue or > int.MaxValue)
        {
            throw ColumnDecodeError.Create<short>(
                columnMetadata,
                "Valid is outside of valid short");
        }
        return (short)value;

    }
    
    public static byte ValidateByte(long value, IColumnMetadata columnMetadata)
    {
        if (value is < int.MinValue or > int.MaxValue)
        {
            throw ColumnDecodeError.Create<byte>(
                columnMetadata,
                "Valid is outside of valid byte");
        }
        return (byte)value;

    }
}
