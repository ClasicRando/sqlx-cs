using System.Buffers;
using System.Net;
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
        ArgumentNullException.ThrowIfNull(address);
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
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <see cref="NetworkUtils.EncodeNetworkValue"/>
    public static void Encode(PgInet value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        NetworkUtils.EncodeNetworkValue<PgInet>(value.Address, value.NetmaskSize, DbType, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <see cref="NetworkUtils.DecodeNetworkValuesAsBytes{T}"/>
    public static PgInet DecodeBytes(in PgBinaryValue value)
    {
        (IPAddress address, var prefix) = NetworkUtils.DecodeNetworkValuesAsBytes<PgInet>(
            in value);
        return new PgInet(address, prefix);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <see cref="NetworkUtils.DecodeNetworkValuesAsText{T}"/>
    public static PgInet DecodeText(in PgTextValue value)
    {
        (IPAddress address, var prefix) = NetworkUtils.DecodeNetworkValuesAsText<PgInet>(
            in value);
        return prefix.HasValue ? new PgInet(address, prefix.Value) : new PgInet(address);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Inet;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.InetArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return NetworkUtils.IsNetworkValueCompatible(typeInfo);
    }
}
