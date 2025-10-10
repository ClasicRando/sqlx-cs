using System.Net;
using System.Net.Sockets;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>INET</c> or <c>CIDR</c> type represented by an address of <see cref="byte"/>s and a
/// <see cref="byte"/> prefix. This type encapsulates IPV4 and IVP6 addresses.
/// </para>
/// <para>
/// Note: This exists since the .NET core library's <see cref="IPAddress"/> does not support
/// prefixes.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-INET">inet docs</a>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-CIDR">cidr docs</a>
/// </summary>
public class PgInet : IPgDbType<PgInet>, IHasArrayType, IEquatable<PgInet>
{
    private const byte PgsqlAfInet = 2;
    private const byte PgsqlAfInet6 = PgsqlAfInet + 1;
    
    private readonly byte[] _address;

    /// <summary>
    /// True if the address is IPV6 (i.e. the address is represented by 16 bytes).
    /// </summary>
    public bool IsIpv6 => _address.Length == 16;

    public ReadOnlySpan<byte> Address => _address;

    public byte Prefix { get; }

    internal PgInet(byte[] address, byte prefix)
    {
        if (address.Length != 4 && address.Length != 16)
        {
            throw new ArgumentException("PgInet addresses must have 4 or 16 bytes for IPV4 or IPV6");
        }
        _address = address;
        Prefix = IsIpv6 switch
        {
            true when prefix > 128 => throw new ArgumentException(
                "PgInet prefix must be <= 128 for IPV6"),
            false when prefix > 32 => throw new ArgumentException(
                "PgInet prefix must be <= 32 for IPV4"),
            _ => prefix,
        };
    }
    
    public PgInet(IPAddress ipAddress, byte prefix) : this(ipAddress.GetAddressBytes(), prefix)
    {
    }

    public PgInet(IPAddress ipAddress) : this(
        ipAddress,
        (byte)(ipAddress.AddressFamily is AddressFamily.InterNetworkV6 ? 128 : 32))
    {
    }

    /// <summary>
    /// Create new <see cref="IPAddress"/> with the <see cref="Address"/> bytes. Prefix is not
    /// recognized in <see cref="IPAddress"/>.
    /// </summary>
    public IPAddress ToIpAddress()
    {
        return new IPAddress(_address);
    }
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
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
    public static void Encode(PgInet value, WriteBuffer buffer)
    {
        var isIpv6 = value.IsIpv6;
        buffer.WriteByte(isIpv6 ? PgsqlAfInet6 : PgsqlAfInet);
        buffer.WriteByte(value.Prefix);
        buffer.WriteByte(0);
        buffer.WriteByte((byte)(isIpv6 ? 16 : 4));
        buffer.WriteBytes(value._address.AsSpan());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
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
    /// address array and prefix, the appropriate <see cref="PgInet"/> instance is created.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/network.c#L292">pg source code</a>
    /// </summary>
    public static PgInet DecodeBytes(PgBinaryValue value)
    {
        var remainingBytes = value.Buffer.Remaining;
        if (remainingBytes < 8)
        {
            throw ColumnDecodeException.Create<PgInet>(
                value.ColumnMetadata,
                $"PgInet values must have at least 8 bytes available. Found {remainingBytes}");
        }

        var family = value.Buffer.ReadByte();
        var prefix = value.Buffer.ReadByte();
        value.Buffer.Skip(2);
        var address = value.Buffer.ReadBytes();
        return family switch
        {
            PgsqlAfInet when address.Length == 4 => new PgInet(address, prefix),
            PgsqlAfInet6 when address.Length == 16 => new PgInet(address, prefix),
            _ => throw ColumnDecodeException.Create<PgInet>(value.ColumnMetadata),
        };
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
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
    public static PgInet DecodeText(PgTextValue value)
    {
        var mid = value.Chars.IndexOf('/');
        if (mid == -1)
        {
            mid = value.Chars.Length;
        }
        
        if (!IPAddress.TryParse(value.Chars[..mid], out IPAddress? ipAddress))
        {
            throw ColumnDecodeException.Create<PgInet>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into an PgInet");
        }

        if (mid == value.Chars.Length)
        {
            return new PgInet(ipAddress);
        }

        if (byte.TryParse(value.Chars[(mid + 1)..], out var parsedPrefix))
        {
            throw ColumnDecodeException.Create<PgInet>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into an PgInet");
        }
        
        return new PgInet(ipAddress, parsedPrefix);
    }

    public static PgType DbType => PgType.Inet;

    public static PgType ArrayDbType => PgType.InetArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid || dbType.TypeOid == PgType.Cidr.TypeOid;
    }

    public static PgType GetActualType(PgInet value)
    {
        return DbType;
    }

    public override string ToString()
    {
        return $"{nameof(PgInet)} {{ {string.Join(':', _address)}/{Prefix} }}";
    }

    public bool Equals(PgInet? other)
    {
        return other is not null
               && _address.SequenceEqual(other._address)
               && Prefix.Equals(other.Prefix);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgInet other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_address, Prefix);
    }
}
