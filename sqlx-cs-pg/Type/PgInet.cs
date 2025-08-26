using System.Net;
using System.Net.Sockets;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public record PgInet : IPgDbType<PgInet>
{
    private const byte PgsqlAfInet = 2;
    private const byte PgsqlAfInet6 = PgsqlAfInet + 1;
    
    private readonly byte[] _address;
    private readonly byte _prefix;

    public bool IsIpv6 => _address.Length == 16;
    
    internal PgInet(byte[] address, byte prefix)
    {
        if (address.Length != 4 && address.Length != 16)
        {
            throw new ArgumentException("PgInet addresses must have 4 or 16 bytes for IPV4 or IPV6");
        }
        _address = address;
        _prefix = IsIpv6 switch
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
    
    public static void Encode(PgInet value, WriteBuffer buffer)
    {
        var isIpv6 = value.IsIpv6;
        buffer.WriteByte(isIpv6 ? PgsqlAfInet6 : PgsqlAfInet);
        buffer.WriteByte(value._prefix);
        buffer.WriteByte(0);
        buffer.WriteByte((byte)(isIpv6 ? 16 : 4));
        buffer.WriteBytes(value._address.AsSpan());
    }

    public static PgInet DecodeBytes(PgBinaryValue value)
    {
        var remainingBytes = value.Buffer.Remaining;
        if (remainingBytes < 8)
        {
            throw ColumnDecodeError.Create<PgInet>(
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
            _ => throw ColumnDecodeError.Create<PgInet>(value.ColumnMetadata),
        };
    }

    public static PgInet DecodeText(PgTextValue value)
    {
        var mid = value.Chars.IndexOf('/');
        if (mid == -1)
        {
            mid = value.Chars.Length;
        }
        
        if (!IPAddress.TryParse(value.Chars[..mid], out IPAddress? ipAddress))
        {
            throw ColumnDecodeError.Create<PgInet>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into an PgInet");
        }

        if (mid == value.Chars.Length)
        {
            return new PgInet(ipAddress);
        }

        if (byte.TryParse(value.Chars[(mid + 1)..], out var parsedPrefix))
        {
            throw ColumnDecodeError.Create<PgInet>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into an PgInet");
        }
        
        return new PgInet(ipAddress, parsedPrefix);
    }

    public static PgType DbType => PgType.Inet;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid || dbType.TypeOid == PgType.Cidr.TypeOid;
    }

    public static PgType GetActualType(PgInet value)
    {
        return DbType;
    }
}
