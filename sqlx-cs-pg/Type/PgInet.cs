using System.Net;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>INET</c> or <c>CIDR</c> type represented by an <see cref="IPAddress"/> and a network
/// mask size as a <see cref="byte"/>. This type encapsulates IPV4 and IVP6 addresses.
/// </para>
/// <para>
/// Note: This exists since the .NET core library's <see cref="IPAddress"/> does not support
/// netmask sizes.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-INET">inet docs</a>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-CIDR">cidr docs</a>
/// </summary>
public readonly record struct PgInet : IPgDbType<PgInet>, IHasArrayType
{
    public IPAddress Address { get; }

    public byte NetmaskSize { get; }

    public PgInet(IPAddress address, byte netmaskSize)
    {
        Address = address;
        NetmaskSize = Address.IsIPv6() switch
        {
            true when netmaskSize > NetworkUtils.MaxIpv6NetmaskSize => throw new ArgumentException(
                $"PgInet prefix must be <= {NetworkUtils.MaxIpv6NetmaskSize} for IPV6"),
            false when netmaskSize > NetworkUtils.MaxIpv4NetmaskSize => throw new ArgumentException(
                $"PgInet prefix must be <= {NetworkUtils.MaxIpv4NetmaskSize} for IPV4"),
            _ => netmaskSize,
        };
    }

    public PgInet(IPAddress ipAddress) : this(
        ipAddress,
        NetworkUtils.GetDefaultNetworkMaskSize(ipAddress))
    {
    }

    public static implicit operator PgInet(IPAddress address) => new(address);

    public static implicit operator IPAddress(PgInet inet) => inet.Address;
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <see cref="NetworkUtils.EncodeNetworkValue"/>
    public static void Encode(PgInet value, WriteBuffer buffer)
    {
        NetworkUtils.EncodeNetworkValue<PgInet>(value.Address, value.NetmaskSize, DbType, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <see cref="NetworkUtils.DecodeNetworkValuesAsBytes{T}"/>
    public static PgInet DecodeBytes(ref PgBinaryValue value)
    {
        (IPAddress address, var prefix) = NetworkUtils.DecodeNetworkValuesAsBytes<PgInet>(
            ref value);
        return new PgInet(address, prefix);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <see cref="NetworkUtils.DecodeNetworkValuesAsText{T}"/>
    public static PgInet DecodeText(PgTextValue value)
    {
        (IPAddress address, var prefix) = NetworkUtils.DecodeNetworkValuesAsText<PgInet>(
            ref value);
        return prefix.HasValue ? new PgInet(address, prefix.Value) : new PgInet(address);
    }

    public static PgType DbType => PgType.Inet;

    public static PgType ArrayDbType => PgType.InetArray;

    public static bool IsCompatible(PgType dbType)
    {
        return NetworkUtils.IsNetworkValueCompatible(dbType);
    }

    public static PgType GetActualType(PgInet value)
    {
        return DbType;
    }
}
