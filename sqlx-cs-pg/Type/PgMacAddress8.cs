using System.Buffers;
using System.Net.NetworkInformation;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>MACADDR8</c> type represented the bytes of a MacAddress
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-MACADDR8">docs</a>
/// </summary>
public readonly record struct PgMacAddress8(
    byte A,
    byte B,
    byte C,
    byte D,
    byte E,
    byte F,
    byte G,
    byte H) : IPgDbType<PgMacAddress8>, IHasArrayType, IPgMacAddress
{
    public PhysicalAddress ToPhysicalAddress()
    {
        return new PhysicalAddress([A, B, C, D, E, F, G, H]);
    }

    public override string ToString()
    {
        return $"{A:X2}:{B:X2}:{C:X2}:{D:X2}:{E:X2}:{F:X2}:{G:X2}:{H:X2}";
    }

    internal static PgMacAddress8 FromBytes(ReadOnlySpan<byte> bytes)
    {
        return bytes switch
        {
            [var a, var b, var c, var d, var e, var f, var g, var h] => new PgMacAddress8(
                a,
                b,
                c,
                d,
                e,
                f,
                g,
                h),
            _ => throw new ArgumentException(
                "Invalid number of bytes supplied to MacAddr8",
                nameof(bytes)),
        };
    }

    public static implicit operator PgMacAddress8(PhysicalAddress address) =>
        FromPhysicalAddress(address);

    public static PgMacAddress8 FromPhysicalAddress(PhysicalAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return FromBytes(address.GetAddressBytes());
    }

    public static implicit operator PhysicalAddress(PgMacAddress8 address) =>
        address.ToPhysicalAddress();

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes all the bytes of the <see cref="PgMacAddress"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac8.c#L253">pg source code - macaddr8</a>
    /// </summary>
    public static void Encode(PgMacAddress8 value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.WriteByte(value.A);
        buffer.WriteByte(value.B);
        buffer.WriteByte(value.C);
        buffer.WriteByte(value.D);
        buffer.WriteByte(value.E);
        buffer.WriteByte(value.F);
        buffer.WriteByte(value.G);
        buffer.WriteByte(value.H);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Check the number of available bytes in the buffer to confirm it has 8 <see cref="byte"/>s.
    /// If that is true, a new <see cref="PgMacAddress8"/> in created using the bytes.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac.c#L161">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the number of available bytes is not 8
    /// </exception>
    public static PgMacAddress8 DecodeBytes(ref PgBinaryValue value)
    {
        var byteCount = value.Buffer.Length;
        if (byteCount != 8)
        {
            throw ColumnDecodeException.Create<PgMacAddress8>(
                value.ColumnMetadata,
                $"Expected 8 bytes. Found {byteCount}");
        }

        return FromBytes(value.Buffer);
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
    public static PgMacAddress8 DecodeText(PgTextValue value)
    {
        Span<Range> splits = stackalloc Range[8];
        var splitCount = value.Chars.Split(splits, ':');
        if (splitCount != 8)
        {
            throw ColumnDecodeException.Create<PgMacAddress8>(
                value.ColumnMetadata,
                $"Expected 8 address hex characters. Found {splitCount}");
        }

        Span<byte> bytes = stackalloc byte[8];
        for (var i = 0; i < splitCount; i++)
        {
            Range rng = splits[i];
            if (rng.End.Value - rng.Start.Value != 2)
            {
                throw ColumnDecodeException.Create<PgMacAddress8>(
                    value.ColumnMetadata,
                    $"Could not parse network location bytes from '{value.Chars}'");
            }

            bytes[i] = (byte)HexUtils.CharsToDigit<PgMacAddress8>(
                value.Chars[rng],
                value.ColumnMetadata);
        }

        bytes = bytes[..splitCount];
        return FromBytes(bytes);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Macaddr8;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.Macaddr8Array;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }
}
