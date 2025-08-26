using System.Runtime.InteropServices;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgDecimal : IPgDbType<decimal>
{
    private const int DecimalBits = 4;
    
    private const ushort SignNan = 0xc000;
    private const ushort SignPositive = 0x0000;
    private const ushort SignNegative = 0x4000;
    private const ushort SignPinf = 0xD000;
    private const ushort SignNinf = 0xF000;

    private const uint NumericBase = 10000;
    private const int NumericBaseLog10 = 4; // log10(10000)

    private const int MaxDecimalNumericDigits = 8;
    private const int MaxDecimalScale = 28;

    // Fast access for 10^n where n is 0-9
    private static ReadOnlySpan<uint> UIntPowers10 =>
    [
        1,
        10,
        100,
        1000,
        10000,
        100000,
        1000000,
        10000000,
        100000000,
        1000000000,
    ];

    private const int MaxUInt32Scale = 9;
    private const int MaxUInt16Scale = 4;
    
    public static void Encode(decimal value, WriteBuffer buffer)
    {
        EncodeDecimal(value, buffer);
    }

    public static decimal DecodeBytes(PgBinaryValue value)
    {
        var digitCount = value.Buffer.ReadShort();
        var weight = value.Buffer.ReadShort();
        var sign = value.Buffer.ReadShort();
        var scale = value.Buffer.ReadShort();

        if ((ushort)sign == SignNan)
        {
            throw ColumnDecodeError.Create<decimal>(
                value.ColumnMetadata,
                "Cannot decode NAN as decimal");
        }

        Span<short> digits = stackalloc short[digitCount];
        for (var i = 0; i < digits.Length; i++)
        {
            digits[i] = value.Buffer.ReadShort();
        }
        return CreateDecimal(digits, weight, sign, scale, value.ColumnMetadata);
    }

    public static decimal DecodeText(PgTextValue value)
    {
        if (!decimal.TryParse(value, null, out var result))
        {
            throw ColumnDecodeError.Create<decimal>(
                value.ColumnMetadata,
                $"Cannot convert '{value}' to a decimal value");
        }

        return result;
    }
    
    public static PgType DbType => PgType.Numeric;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(decimal value)
    {
        return DbType;
    }
    
    /// <summary>
    /// Conversion method from Postgres numeric value to a <see cref="decimal"/>. Code is taken
    /// mostly as is from the npgsql repo with some small changes since there is no wrapper type,
    /// and we throw our own custom exceptions.
    /// <p>
    /// Copyright (c) 2002-2025, Npgsql
    /// 
    /// Permission to use, copy, modify, and distribute this software and its
    /// documentation for any purpose, without fee, and without a written agreement
    /// is hereby granted, provided that the above copyright notice and this
    /// paragraph and the following two paragraphs appear in all copies.
    /// 
    /// IN NO EVENT SHALL NPGSQL BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT,
    /// SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS,
    /// ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF
    /// Npgsql HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    /// 
    /// NPGSQL SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED
    /// TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
    /// PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS ON AN "AS IS" BASIS, AND Npgsql
    /// HAS NO OBLIGATIONS TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS,
    /// OR MODIFICATIONS.
    /// </p>
    /// </summary>
    /// <a href="https://github.com/npgsql/npgsql/blob/19f466e3e12106b9e7a81e67d07c4df56467a861/src/Npgsql/Internal/Converters/Primitive/PgNumeric.cs#L295"></a>
    /// <param name="digits"></param>
    /// <param name="weight"></param>
    /// <param name="sign"></param>
    /// <param name="scale"></param>
    /// <param name="columnMetadata"></param>
    /// <returns></returns>
    /// <exception cref="ColumnDecodeError"></exception>
    private static decimal CreateDecimal(
        ReadOnlySpan<short> digits,
        short weight,
        short sign,
        short scale,
        PgColumnMetadata columnMetadata)
    {
        var digitCount = digits.Length;
        if (digitCount > MaxDecimalNumericDigits)
        {
            throw ColumnDecodeError.Create<decimal>(
                columnMetadata,
                "Numeric value does not fit into a decimal");
        }

        if (short.Abs(scale) > MaxDecimalScale)
        {
            throw ColumnDecodeError.Create<decimal>(
                columnMetadata,
                "Numeric value does not fit into a decimal");
        }
        
        var scaleFactor = new decimal(1, 0, 0, false, (byte)(scale > 0 ? scale : 0));
        if (digitCount == 0)
        {
            return (ushort)sign switch
            {
                SignPositive or SignNegative => decimal.Zero * scaleFactor,
                SignNan => throw ColumnDecodeError.Create<decimal>(
                    columnMetadata, 
                    "Numeric value of NaN is not supported by decimal"),
                SignPinf => throw ColumnDecodeError.Create<decimal>(
                    columnMetadata, 
                    "Numeric value of Infinity is not supported by decimal"),
                SignNinf => throw ColumnDecodeError.Create<decimal>(
                    columnMetadata, 
                    "Numeric value of -Infinity is not supported by decimal"),
                _ => throw ColumnDecodeError.Create<decimal>(
                    columnMetadata,
                    $"Sign code of {(ushort)sign} is not supported"),
            };
        }

        var numericBase = new decimal(NumericBase);
        var result = decimal.Zero;
        for (var i = 0; i < digits.Length - 1; i++)
        {
            result *= numericBase;
            result += digits[i];
        }

        var digitScale = (weight + 1 - digitCount) * NumericBaseLog10;
        var scaleDifference = scale < 0 ? digitCount : digitScale + scale;

        var digit = digits[digitCount - 1];
        if (digitCount == MaxDecimalNumericDigits)
        {
            var pow = UIntPowers10[-scaleDifference];
            result *= numericBase / pow;
            result += new decimal(digit / pow);
        }
        else
        {
            result *= numericBase;
            result += digit;

            if (scaleDifference < 0)
            {
                while (scaleDifference < 0)
                {
                    var scaleChunk = Math.Min(MaxUInt16Scale, -scaleDifference);
                    scaleFactor /= UIntPowers10[scaleChunk];
                    scaleDifference += scaleChunk;
                }
            }
            else
            {
                while (scaleDifference > 0)
                {
                    var scaleChunk = Math.Min(MaxUInt16Scale, scaleDifference);
                    scaleFactor *= UIntPowers10[scaleChunk];
                    scaleDifference -= scaleChunk;
                }
            }
        }

        result *= scaleFactor;
        return (ushort)sign == SignNegative ? -result : result;
    }
    
    /// <summary>
    /// Encoding method to write a <see cref="decimal"/> value in Postgres numeric format. Code is
    /// taken mostly as is from the npgsql repo with some small changes since there is no wrapper
    /// type, and we throw our own custom exceptions.
    /// <p>
    /// Copyright (c) 2002-2025, Npgsql
    /// 
    /// Permission to use, copy, modify, and distribute this software and its
    /// documentation for any purpose, without fee, and without a written agreement
    /// is hereby granted, provided that the above copyright notice and this
    /// paragraph and the following two paragraphs appear in all copies.
    /// 
    /// IN NO EVENT SHALL NPGSQL BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT,
    /// SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS,
    /// ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF
    /// Npgsql HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    /// 
    /// NPGSQL SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED
    /// TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
    /// PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS ON AN "AS IS" BASIS, AND Npgsql
    /// HAS NO OBLIGATIONS TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS,
    /// OR MODIFICATIONS.
    /// </p>
    /// </summary>
    /// <a href="https://github.com/npgsql/npgsql/blob/19f466e3e12106b9e7a81e67d07c4df56467a861/src/Npgsql/Internal/Converters/Primitive/PgNumeric.cs#L295"></a>
    /// <param name="value"></param>
    /// <param name="buffer"></param>
    private static void EncodeDecimal(decimal value, WriteBuffer buffer)
    {
        Span<uint> bits = stackalloc uint[DecimalBits];
        decimal.GetBits(value, MemoryMarshal.Cast<uint, int>(bits));
        bits = bits[..(DecimalBits - 1)];
        var bitsUpperBound = (bits.Length * (MaxUInt32Scale + 1) + MaxUInt16Scale - 1) / MaxUInt16Scale + 1;
        
        Span<short> digits = stackalloc short[bitsUpperBound];
        short scale = value.Scale;

        var digitCount = 0;
        var digitWeight = -scale / NumericBaseLog10 - 1;
        var scaleRemainder = scale % NumericBaseLog10;
        if (scaleRemainder > 0 && DivideInPlace(bits, UIntPowers10[scaleRemainder], out var remainder))
        {
            remainder *= UIntPowers10[NumericBaseLog10 - scaleRemainder];
            digitWeight--;
            digits[^1] = (short)remainder;
            digitCount++;
        }

        while (DivideInPlace(bits, NumericBase, out remainder))
        {
            if (digitCount == 0 && remainder == 0)
            {
                digitWeight++;
            }
            else
            {
                digits[digits.Length - 1 - digitCount++] = (short)remainder;
            }
        }

        var weight = (short)(digitWeight + digitCount);
        var sign = value < 0 ? SignNegative : SignPositive;
        digits = digits[^digitCount..];
        
        buffer.WriteShort((short)digits.Length);
        buffer.WriteShort(weight);
        buffer.WriteShort((short)sign);
        buffer.WriteShort(scale);
        foreach (var digit in digits)
        {
            buffer.WriteShort(digit);
        }
    }

    private static bool DivideInPlace(Span<uint> left, uint right, out uint remainder)
    {
        var carry = 0UL;

        var nonZeroInput = false;
        for (var i = left.Length - 1; i >= 0; i--)
        {
            var value = (carry << 32) | left[i];
            nonZeroInput = nonZeroInput || value != 0;
            var digit = value / right;
            left[i] = (uint)digit;
            carry = value - digit * right;
        }

        remainder = (uint)carry;
        return nonZeroInput;
    }
}
