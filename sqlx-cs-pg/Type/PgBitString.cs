using System.Buffers;
using System.Collections;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// <see cref="IPgDbType{T}"/> for <see cref="BitArray"/> values. Maps to the <c>VARBIT(n)</c> and
/// <c>BIT(n)</c> types.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-bit.html">docs</a>
/// </summary>
public abstract class PgBitString : IPgDbType<BitArray>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Simply writes a 1 or 0 for true or false respectively.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/d57b7cc3338e9d9aa1d7c5da1b25a17c5a72dcce/src/backend/utils/adt/varbit.c#L636">pg source code</a>
    /// </summary>
    public static void Encode(BitArray value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(buffer);
        var byteCount = GetByteCountFromBitCount(value.Length);
        buffer.WriteInt(value.Length);
        byte[]? borrowedArray = null;
        var bytes = value.Length > 256
            ? (borrowedArray = ArrayPool<byte>.Shared.Rent(byteCount))
            : stackalloc byte[byteCount];

        try
        {
            for (var i = 0; i < byteCount; i++)
            {
                var bitPosition = i * 8;
                var bits = int.Min(8, value.Length - bitPosition);
                var currentByte = 0;
                for (var j = 0; j < bits; j++)
                {
                    currentByte += (value[bitPosition + j] ? 1 : 0) << (8 - j - 1);
                }

                bytes[i] = (byte) currentByte;
            }
            
            buffer.Write(bytes);
        }
        finally
        {
            if (borrowedArray != null)
            {
                ArrayPool<byte>.Shared.Return(borrowedArray);
            }
        }
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Read the number of bits then the remaining bytes as the bytes stored in the
    /// <see cref="BitArray"/>. Before the bytes are provided to the <see cref="BitArray"/> they
    /// must first be reversed (using a bit hack found in the Npgsql repo). The returned
    /// <see cref="BitArray"/> must also be trimmed to the desired bit length.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/d57b7cc3338e9d9aa1d7c5da1b25a17c5a72dcce/src/backend/utils/adt/varbit.c#L681">pg source code</a>
    /// </summary>
    public static BitArray DecodeBytes(ref PgBinaryValue value)
    {
        var bitsLength = value.Buffer.ReadInt();
        var bytesLength = GetByteCountFromBitCount(bitsLength);

        if (bytesLength != value.Buffer.Length)
        {
            throw ColumnDecodeException.Create<BitArray, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Expected buffer to contain {bytesLength} bytes but found {value.Buffer.Length}");
        }

        var bytes = value.Buffer.ReadBytes();

        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = ReverseBits(bytes[i]);
        }
        
        return new BitArray(bytes)
        {
            Length = bitsLength,
        };

        // https://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
        static byte ReverseBits(byte b) =>
            (byte)(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// The chars provided are interpreted as an array of boolean 0s and 1s to construct a
    /// <see cref="BitArray"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/a6c21887a9f0251fa2331ea3ad0dd20b31c4d11d/src/backend/utils/adt/bool.c#L126">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If any character is not 0 or 1
    /// </exception>
    public static BitArray DecodeText(in PgTextValue value)
    {
        var bitArray = new BitArray(value.Chars.Length);
        
        for (var i = 0; i < value.Chars.Length; i++)
        {
            bitArray[i] = value.Chars[i] switch
            {
                '0' => false,
                '1' => true,
                _ => throw ColumnDecodeException.Create<BitArray, PgColumnMetadata>(
                    value.ColumnMetadata,
                    $"Could not decode char #{i} in {value.Chars}"),
            };
        }

        return bitArray;
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Varbit;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.VarbitArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType || typeInfo == PgTypeInfo.Bit;
    }

    private static int GetByteCountFromBitCount(int bitCount)
    {
        const int bitShiftPerByte = 3;
        return (bitCount - 1 + (1 << bitShiftPerByte)) >>> bitShiftPerByte;
    }
}
