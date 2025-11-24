using System.Net;
using System.Net.Sockets;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal static class NetworkUtils
{
    private const byte PgsqlAfInet = 2;
    private const byte PgsqlAfInet6 = PgsqlAfInet + 1;
    public const byte MaxIpv6NetmaskSize = 128;
    public const byte MaxIpv4NetmaskSize = 32;
    
    /// <summary>
    /// <para>
    /// Writes 5 values to the buffer:
    /// <list type="number">
    ///     <item>
    ///         <see cref="byte"/> - Header to designate value's inet type(IPV4 = PGSQL_AF_INET and
    ///         IPV6 = PGSQL_AF_INET6)
    ///     </item>
    ///     <item><see cref="byte"/> - The prefix of the address</item>
    ///     <item><see cref="byte"/> - Is CIDR flag, always 0</item>
    ///     <item><see cref="byte"/> - The number of following bytes (IPV4 = 4, IPV6 = 16)</item>
    ///     <item><see cref="byte"/>[] - bytes that represent the address</item>
    /// </list>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/network.c#L250">pg source code</a>
    /// </summary>
    public static void EncodeNetworkValue<T>(
        IPAddress value,
        byte prefix,
        PgTypeInfo dataType,
        WriteBuffer buffer)
        where T : notnull
    {
        var isIpv6 = value.AddressFamily is AddressFamily.InterNetworkV6;
        var bytesToWrite = isIpv6 ? 16 : 4;
        buffer.WriteByte(isIpv6 ? PgsqlAfInet6 : PgsqlAfInet);
        buffer.WriteByte(prefix);
        buffer.WriteByte((byte)(dataType == PgTypeInfo.Cidr ? 1 : 0));
        buffer.WriteByte((byte)bytesToWrite);
        var span = buffer.GetSpan(bytesToWrite);
        if (!value.TryWriteBytes(span, out var written))
        {
            throw ColumnEncodeException.Create<T>(
                dataType.TypeOid.Inner,
                "Failed to write address bytes to buffer");
        }

        if (bytesToWrite != written)
        {
            throw ColumnEncodeException.Create<T>(
                dataType.TypeOid.Inner,
                "Wrote a different number of bytes to the parameter buffer than expected");
        }
        buffer.Advance(written);
    }
    
    /// <summary>
    /// <para>
    /// Reads the buffer for the following components:
    /// <list type="number">
    ///     <item>
    ///         <see cref="byte"/> - Header to designate value's inet type(IPV4 = PGSQL_AF_INET and
    ///         IPV6 = PGSQL_AF_INET6)
    ///     </item>
    ///     <item><see cref="byte"/> - The prefix of the address</item>
    ///     <item><see cref="byte"/> - Is CIDR flag, always 0</item>
    ///     <item><see cref="byte"/> - The number of following bytes (IPV4 = 4, IPV6 = 16)</item>
    ///     <item><see cref="byte"/>[] - bytes that represent the address</item>
    /// </list>
    /// </para>
    /// <para>
    /// For IPV4 addresses the number of bytes in the address array and the length must be 4. For
    /// IPV6 addresses the number of bytes in the address array and length must be 16. With the
    /// address array and prefix, the appropriate <see cref="IPAddress"/> instance is created with
    /// the associated prefix.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/network.c#L292">pg source code</a>
    /// </summary>
    public static (IPAddress Address, byte Prefix) DecodeNetworkValuesAsBytes<T>(
        ref PgBinaryValue value)
        where T : notnull
    {
        var remainingBytes = value.Buffer.Remaining;
        if (remainingBytes < 8)
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Network values must have at least 8 bytes available. Found {remainingBytes}");
        }

        var family = value.Buffer.ReadByte();
        var prefix = value.Buffer.ReadByte();
        value.Buffer.Skip(2);
        var span = value.Buffer.ReadBytesAsSpan();
        var address = new IPAddress(span);
        return family switch
        {
            PgsqlAfInet when span.Length == 4 => (address, prefix),
            PgsqlAfInet6 when span.Length == 16 => (address, prefix),
            _ => throw ColumnDecodeException.Create<T>(value.ColumnMetadata),
        };
    }

    /// <summary>
    /// <para>
    /// Attempt to parse the characters to an <see cref="IPAddress"/> and <see cref="byte"/> prefix
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/network.c#L165">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters cannot be parsed to an <see cref="IPAddress"/> or the prefix part is not
    /// a <see cref="byte"/>
    /// </exception>
    public static (IPAddress Address, byte? Prefix) DecodeNetworkValuesAsText<T>(
        ref PgTextValue value)
        where T : notnull
    {
        var mid = value.Chars.IndexOf('/');
        if (mid == -1)
        {
            mid = value.Chars.Length;
        }

        if (!IPAddress.TryParse(value.Chars[..mid], out IPAddress? ipAddress))
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a network value");
        }

        if (mid == value.Chars.Length)
        {
            return (ipAddress, null);
        }

        if (!byte.TryParse(value.Chars[(mid + 1)..], out var parsedPrefix))
        {
            throw ColumnDecodeException.Create<T>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a network value");
        }

        return (ipAddress, parsedPrefix);
    }

    /// <param name="ipAddress">IP address</param>
    /// <returns>Default network mask size, 128 for IPv6, otherwise 32</returns>
    public static byte GetDefaultNetworkMaskSize(IPAddress ipAddress)
    {
        return ipAddress.IsIPv6() ? MaxIpv6NetmaskSize : MaxIpv4NetmaskSize;
    }

    /// <param name="dbType">database type to check for compatability</param>
    /// <returns>True if the <see cref="PgTypeInfo"/> is a network value type</returns>
    public static bool IsNetworkValueCompatible(PgTypeInfo dbType)
    {
        return dbType == PgTypeInfo.Inet || dbType == PgTypeInfo.Cidr;
    }

    /// <returns>True if the address is an IPv6 address</returns>
    public static bool IsIPv6(this IPAddress address)
    {
        return address.AddressFamily is AddressFamily.InterNetworkV6;
    }
}
