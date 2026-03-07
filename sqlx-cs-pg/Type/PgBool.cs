using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// <see cref="IPgDbType{T}"/> for <see cref="bool"/> values. Maps to the <c>BOOLEAN</c> type.
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-boolean.html">docs</a>
/// </summary>
internal abstract class PgBool : IPgDbType<bool>, IHasArrayType
{
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Simply writes a 1 or 0 for true or false respectively.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/a6c21887a9f0251fa2331ea3ad0dd20b31c4d11d/src/backend/utils/adt/bool.c#L174">pg source code</a>
    /// </summary>
    public static void Encode(bool value, IBufferWriter<byte> buffer)
    {
        buffer.WriteByte((byte)(value ? 1 : 0));
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Read the first byte and interpret any non-zero byte as true and a 0 as false
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/a6c21887a9f0251fa2331ea3ad0dd20b31c4d11d/src/backend/utils/adt/bool.c#L187">pg source code</a>
    /// </summary>
    public static bool DecodeBytes(ref PgBinaryValue value)
    {
        return value.Buffer.ReadByte() != 0;
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Interpret the first character as 't' for true and 'f' as false
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/a6c21887a9f0251fa2331ea3ad0dd20b31c4d11d/src/backend/utils/adt/bool.c#L126">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the first character is not 't' or 'f'
    /// </exception>
    public static bool DecodeText(in PgTextValue value)
    {
        return value.Chars[0] switch
        {
            't' => true,
            'f' => false,
            _ => throw ColumnDecodeException.Create<bool, PgColumnMetadata>(
                value.ColumnMetadata,
                $"First character must be 't' or 'f'. Found '{value.Chars}'"),
        };
    }

    public static PgTypeInfo DbType => PgTypeInfo.Bool;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.BoolArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }
}
