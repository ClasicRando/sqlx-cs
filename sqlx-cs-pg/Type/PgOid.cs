using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Types;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly struct PgOid(uint inner) : IEquatable<PgOid>, IPgDbType<PgOid>, IHasArrayType
{
    public uint Inner { get; } = inner;
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// Writes the <see cref="uint"/> value to the buffer
    /// </summary>
    public static void Encode(PgOid value, IBufferWriter<byte> buffer)
    {
        buffer.WriteUInt(value.Inner);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// Read an <see cref="uint"/> value from the buffer. Down casts if the actual value is a
    /// <see cref="long"/> but can be safely fit within an <see cref="uint"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the integer value is outside a valid <see cref="uint"/>
    /// </exception>
    public static PgOid DecodeBytes(ref PgBinaryValue value)
    {
        var integer = value.ExtractInteger<uint>();
        return new PgOid(Integers.ValidateUInt(integer, value.ColumnMetadata));
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// Parse the characters to an <see cref="uint"/> value. Down casts if the actual value is a
    /// <see cref="long"/> but can be safely fit within an <see cref="uint"/>.
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters are not an <see cref="uint"/> value
    /// </exception>
    public static PgOid DecodeText(PgTextValue value)
    {
        var integer = value.ExtractInteger<uint>();
        return new PgOid(Integers.ValidateUInt(integer, value.ColumnMetadata));
    }

    public static PgTypeInfo DbType => PgTypeInfo.Oid;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.OidArray;
    
    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return PgInteger.IsIntegerCompatible(dbType);
    }

    public bool Equals(PgOid other)
    {
        return Inner == other.Inner;
    }

    public override bool Equals(object? obj)
    {
        return obj is PgOid other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Inner);
    }

    public override string ToString()
    {
        return $"{nameof(PgOid)}({Inner})";
    }

    public static bool operator ==(PgOid left, PgOid right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgOid left, PgOid right)
    {
        return !left.Equals(right);
    }
}
