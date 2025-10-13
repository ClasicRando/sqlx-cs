using System.Net.NetworkInformation;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>MACADDR</c> and <c>MACADDR8</c> type represented the bytes of a MacAddress
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-MACADDR">macaddr docs</a>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-MACADDR8">macaddr8 docs</a>
/// </summary>
public readonly record struct PgMacAddress(
    byte A,
    byte B,
    byte C,
    byte D,
    byte E,
    byte F,
    byte G,
    byte H) : IPgDbType<PgMacAddress>, IHasArrayType
{
    private const byte DefaultD = 0xFF;
    private const byte DefaultE = 0xFE;
    
    public PgMacAddress(
        byte A,
        byte B,
        byte C,
        byte F,
        byte G,
        byte H) : this(A, B, C, DefaultD, DefaultE, F, G, H)
    {
        IsMacAddress8 = false;
    }
    
    public bool IsMacAddress8 { get; } = true;

    public PgMacAddress ToMacAddr()
    {
        return new PgMacAddress(A, B, C, F, G, H);
    }

    public PhysicalAddress ToPhysicalAddress()
    {
        return new PhysicalAddress(IsMacAddress8 ? [A, B, C, D, E, F, G, H] : [A, B, C, F, G, H]);
    }

    public override string ToString()
    {
        return $"{A:X2}:{B:X2}:{C:X2}:{D:X2}:{E:X2}:{F:X2}:{G:X2}:{H:X2}";
    }

    internal static PgMacAddress FromBytes(ReadOnlySpan<byte> bytes)
    {
        return bytes switch
        {
            [var a, var b, var c, var d, var e, var f, var g, var h] => new PgMacAddress(
                a,
                b,
                c,
                d,
                e,
                f,
                g,
                h),
            [var a, var b, var c, var f, var g, var h] => new PgMacAddress(
                a,
                b,
                c,
                f,
                g,
                h),
            _ => throw new ArgumentException(
                "Invalid number of bytes supplied to MacAddr",
                nameof(bytes)),
        };
    }
    
    public static implicit operator PgMacAddress(PhysicalAddress address)
        => FromBytes(address.GetAddressBytes());

    public static implicit operator PhysicalAddress(PgMacAddress address) => address.ToPhysicalAddress();

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes all the bytes of the <see cref="PgMacAddress"/> unless
    /// <see cref="PgMacAddress.IsMacAddress8"/> returns true, in which case only the first and last
    /// 3 bytes are written.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac.c#L140">pg source code - macaddr</a>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac8.c#L253">pg source code - macaddr8</a>
    /// </summary>
    public static void Encode(PgMacAddress value, WriteBuffer buffer)
    {
        buffer.WriteByte(value.A);
        buffer.WriteByte(value.B);
        buffer.WriteByte(value.C);
        if (value.IsMacAddress8)
        {
            buffer.WriteByte(value.D);
            buffer.WriteByte(value.E);
        }
        buffer.WriteByte(value.F);
        buffer.WriteByte(value.G);
        buffer.WriteByte(value.H);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Check the number of available bytes in the buffer to confirm it either has 6 or 8
    /// <see cref="byte"/>s. If the buffer has 6 bytes then the 4th and 5th bytes that are required
    /// for a <see cref="PgMacAddress"/> are filled in as 0xFF and 0xFE (follows the postgresql
    /// internal behaviour).
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac.c#L161">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the number of available bytes is not 6 or 8
    /// </exception>
    public static PgMacAddress DecodeBytes(ref PgBinaryValue value)
    {
        var byteCount = value.Buffer.Remaining;
        if (byteCount != 6 && byteCount != 8)
        {
            throw ColumnDecodeException.Create<PgMacAddress>(
                value.ColumnMetadata,
                $"Expected 6 or 8 bytes. Found {byteCount}");
        }

        var bytes = value.Buffer.ReadBytesAsSpan();
        return FromBytes(bytes);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Splits the characters by ':' to get each hex value of the MacAddress. Each hex literal is
    /// then converted using <see cref="HexUtils.CharsToDigit"/> and all the bytes captures in a new
    /// instance of <see cref="PgMacAddress"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac.c#L121">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters do not match the expected format of a Postgres MacAddress literal
    /// </exception>
    public static PgMacAddress DecodeText(PgTextValue value)
    {
        Span<Range> splits = stackalloc Range[8];
        var splitCount = value.Chars.Split(splits, ':');
        if (splitCount != 6 && splitCount != 8)
        {
            throw ColumnDecodeException.Create<PgMacAddress>(
                value.ColumnMetadata,
                $"Expected 6 or 8 address hex characters. Found {splitCount}");
        }

        Span<byte> bytes = stackalloc byte[8];
        for (var i = 0; i < splitCount; i++)
        {
            Range rng = splits[i];
            if (rng.End.Value - rng.Start.Value != 2)
            {
                throw ColumnDecodeException.Create<PgMacAddress>(
                    value.ColumnMetadata,
                    $"Could not parse network location bytes from '{value}'");
            }

            bytes[i] = (byte)HexUtils.CharsToDigit<PgMacAddress>(
                value.Chars[rng],
                value.ColumnMetadata);
        }

        bytes = bytes[..splitCount];
        return FromBytes(bytes);
    }

    public static PgType DbType => PgType.Macaddr;

    public static PgType ArrayDbType => PgType.MacaddrArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType == DbType || dbType == PgType.Macaddr8;
    }

    public static PgType GetActualType(PgMacAddress value)
    {
        return value.IsMacAddress8 ? PgType.Macaddr8 : PgType.Macaddr;
    }
}
