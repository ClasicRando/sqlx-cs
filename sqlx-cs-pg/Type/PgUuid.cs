using System.Buffers;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for <see cref="Guid"/> values. Maps to the <c>UUID</c> type.
/// </summary>
public abstract class PgUuid : IPgDbType<Guid>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the bytes of the <see cref="Guid"/> using
    /// <see cref="Guid.TryWriteBytes(Span{byte}, bool, out int)"/>.
    /// </summary>
    public static void Encode(Guid value, IBufferWriter<byte> buffer)
    {
        var span = buffer.GetSpan(16);
        if (!value.TryWriteBytes(span, bigEndian: false, out _))
        {
            throw ColumnEncodeException.Create<Guid>(
                DbType.TypeOid.Inner,
                "Failed to write Guid bytes to buffer");
        }
        buffer.Advance(16);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read the all available bytes as a new <see cref="Guid"/>
    /// </summary>
    public static Guid DecodeBytes(ref PgBinaryValue value)
    {
        return new Guid(value.Buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Parse the characters using
    /// <see cref="Guid.TryParse(ReadOnlySpan{char}, IFormatProvider, out Guid)"/>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters cannot be parsed into a <see cref="Guid"/>
    /// </exception>
    public static Guid DecodeText(PgTextValue value)
    {
        if (!Guid.TryParse(value, out Guid guid))
        {
            throw ColumnDecodeException.Create<Guid>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a Guid");
        }

        return guid;
    }

    public static PgTypeInfo DbType => PgTypeInfo.Uuid;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.UuidArray;

    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return dbType == DbType;
    }
}
