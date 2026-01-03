using System.Buffers;
using System.Net;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// <see cref="IPgDbType{T}"/> for <see cref="IPNetwork"/> values. Maps to the <c>CIDR</c> but is
/// also compatible with the <c>INET</c> type. Note that decoding an <c>INET</c> value to and
/// <see cref="IPNetwork"/> might fail if the actual value is not a valid <c>CIDR</c>. See postgres
/// docs for differences.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-INET">inet docs</a>
/// <a href="https://www.postgresql.org/docs/current/datatype-net-types.html#DATATYPE-CIDR">cidr docs</a>
/// </summary>
internal abstract class PgIpNetwork : IPgDbType<IPNetwork>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <see cref="NetworkUtils.EncodeNetworkValue"/>
    public static void Encode(IPNetwork value, IBufferWriter<byte> buffer)
    {
        NetworkUtils.EncodeNetworkValue<IPNetwork>(
            value.BaseAddress,
            (byte)value.PrefixLength,
            DbType,
            buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <see cref="NetworkUtils.DecodeNetworkValuesAsBytes{T}"/>
    public static IPNetwork DecodeBytes(ref PgBinaryValue value)
    {
        (IPAddress address, var prefix) = NetworkUtils.DecodeNetworkValuesAsBytes<IPNetwork>(
            ref value);
        return new IPNetwork(address, prefix);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <see cref="NetworkUtils.DecodeNetworkValuesAsText{T}"/>
    public static IPNetwork DecodeText(PgTextValue value)
    {
        (IPAddress address, var prefix) = NetworkUtils.DecodeNetworkValuesAsText<IPNetwork>(
            ref value);
        return new IPNetwork(address, prefix ?? NetworkUtils.GetDefaultNetworkMaskSize(address));
    }

    public static PgTypeInfo DbType => PgTypeInfo.Cidr;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.CidrArray;

    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return NetworkUtils.IsNetworkValueCompatible(dbType);
    }
}
