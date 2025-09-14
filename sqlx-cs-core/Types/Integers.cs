using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

public static class Integers
{
    /// <summary>
    /// Check to see if this <see cref="long"/> value is a valid int
    /// </summary>
    /// <param name="value">long value to check</param>
    /// <param name="columnMetadata">column metadata to construct the exception</param>
    /// <returns>the value cast to an int</returns>
    /// <exception cref="ColumnDecodeException">if the value is not a valid int</exception>
    public static int ValidateInt(long value, IColumnMetadata columnMetadata)
    {
        ColumnDecodeException.CheckOrThrow<int>(
            value is >= int.MinValue and <= int.MaxValue,
            columnMetadata,
            "Value is outside of valid int");
        return (int)value;
    }
    
    /// <summary>
    /// Check to see if this <see cref="long"/> value is a valid short
    /// </summary>
    /// <param name="value">long value to check</param>
    /// <param name="columnMetadata">column metadata to construct the exception</param>
    /// <returns>the value cast to a short</returns>
    /// <exception cref="ColumnDecodeException">if the value is not a valid short</exception>
    public static short ValidateShort(long value, IColumnMetadata columnMetadata)
    {
        ColumnDecodeException.CheckOrThrow<short>(
            value is >= short.MinValue and <= short.MaxValue,
            columnMetadata,
            "Value is outside of valid short");
        return (short)value;
    }
    
    /// <summary>
    /// Check to see if this <see cref="long"/> value is a valid byte
    /// </summary>
    /// <param name="value">long value to check</param>
    /// <param name="columnMetadata">column metadata to construct the exception</param>
    /// <returns>the value cast to a byte</returns>
    /// <exception cref="ColumnDecodeException">if the value is not a valid byte</exception>
    public static byte ValidateByte(long value, IColumnMetadata columnMetadata)
    {
        ColumnDecodeException.CheckOrThrow<byte>(
            value is >= byte.MinValue and <= byte.MaxValue,
            columnMetadata,
            "Value is outside of valid byte");
        return (byte)value;
    }
}
