using System.Buffers;
using System.Net.NetworkInformation;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>MACADDR</c> type represented the bytes of a MacAddress
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-MACADDR">macaddr docs</a>
/// </summary>
public readonly record struct PgMacAddress(
    byte A,
    byte B,
    byte C,
    byte D,
    byte E,
    byte F) : IPgDbType<PgMacAddress>, IHasArrayType, IPgMacAddress
{
    public PhysicalAddress ToPhysicalAddress()
    {
        return new PhysicalAddress([A, B, C, F, E, F]);
    }

    public override string ToString()
    {
        return $"{A:X2}:{B:X2}:{C:X2}:{D:X2}:{E:X2}:{F:X2}";
    }

    internal static PgMacAddress FromBytes(ReadOnlySpan<byte> bytes)
    {
        return bytes switch
        {
            [var a, var b, var c, var d, var e, var f] => new PgMacAddress(
                a,
                b,
                c,
                d,
                e,
                f),
            _ => throw new ArgumentException(
                "Invalid number of bytes supplied to MacAddr",
                nameof(bytes)),
        };
    }

    public static implicit operator PgMacAddress(PhysicalAddress address) =>
        FromPhysicalAddress(address);

    public static PgMacAddress FromPhysicalAddress(PhysicalAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return FromBytes(address.GetAddressBytes());
    }

    public static implicit operator PhysicalAddress(PgMacAddress address) =>
        address.ToPhysicalAddress();

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes all the bytes of the <see cref="PgMacAddress"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac.c#L140">pg source code - macaddr</a>
    /// </summary>
    public static void Encode(PgMacAddress value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.WriteByte(value.A);
        buffer.WriteByte(value.B);
        buffer.WriteByte(value.C);
        buffer.WriteByte(value.D);
        buffer.WriteByte(value.E);
        buffer.WriteByte(value.F);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Check the number of available bytes in the buffer to confirm it has 6 <see cref="byte"/>s.
    /// If that is true, a new <see cref="PgMacAddress"/> in created using the bytes.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/mac.c#L161">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the number of available bytes is not 6
    /// </exception>
    public static PgMacAddress DecodeBytes(ref PgBinaryValue value)
    {
        var byteCount = value.Buffer.Length;
        if (byteCount != 6)
        {
            throw ColumnDecodeException.Create<PgMacAddress, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Expected 6 bytes. Found {byteCount}");
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
    public static PgMacAddress DecodeText(in PgTextValue value)
    {
        Span<Range> splits = stackalloc Range[6];
        var splitCount = value.Chars.Split(splits, ':');
        if (splitCount != 6)
        {
            throw ColumnDecodeException.Create<PgMacAddress, PgColumnMetadata>(
                value.ColumnMetadata,
                $"Expected 6 address hex characters. Found {splitCount}");
        }

        Span<byte> bytes = stackalloc byte[6];
        for (var i = 0; i < splitCount; i++)
        {
            Range rng = splits[i];
            if (rng.End.Value - rng.Start.Value != 2)
            {
                throw ColumnDecodeException.Create<PgMacAddress, PgColumnMetadata>(
                    value.ColumnMetadata,
                    $"Could not parse network location bytes from '{value.Chars}'");
            }

            bytes[i] = (byte)HexUtils.CharsToDigit<PgMacAddress>(
                value.Chars[rng],
                value.ColumnMetadata);
        }

        bytes = bytes[..splitCount];
        return FromBytes(bytes);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Macaddr;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.MacaddrArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }
}
