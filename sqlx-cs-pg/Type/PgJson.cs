using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Types;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <see cref="IPgDbType{T}"/> for a JSON values of type <typeparamref name="T"/>. Maps to the
/// <c>JSONB</c>/<c>JSON</c> type.
/// </summary>
internal abstract class PgJson<T> : IPgDbType<T>, IHasArrayType where T : notnull
{
    private const byte JsonBVersion = 1;
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Encode a value of type <typeparamref name="T"/> as JSON by deferring to
    /// <see cref="Encode(T, IBufferWriter{byte}, JsonTypeInfo{T})"/>. This method always uses
    /// runtime JSON serialization which is slower and uses more memory when compared to JSON
    /// serialization with source generation.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/jsonb.c#L93">pg source code</a>
    /// </summary>
    public static void Encode(T value, IBufferWriter<byte> buffer)
    {
        Encode(value, buffer, null);
    }
    
    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Encode a value of type <typeparamref name="T"/> as JSON. Writes the JSONB version number
    /// (always 1) to the buffer before writing the value as JSON using
    /// <see cref="Json.WriteToBuffer"/> to encode the value.
    /// </para>
    /// <para>
    /// This method allows for passing
    /// <see cref="JsonTypeInfo{T}"/> which generally speeds up serialization since it knows type
    /// information that would otherwise require reflection at runtime.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/jsonb.c#L93">pg source code</a>
    /// </summary>
    public static void Encode(T value, IBufferWriter<byte> buffer, JsonTypeInfo<T>? typeInfo)
    {
        buffer.WriteByte(JsonBVersion);
        Json.WriteToBuffer(buffer, value, typeInfo);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Decode a value of type <typeparamref name="T"/> as JSON by deferring to
    /// <see cref="DecodeBytes(ref PgBinaryValue, JsonTypeInfo{T})"/>. This method always uses
    /// runtime JSON serialization which is slower and uses more memory when compared to JSON
    /// serialization with source generation.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/jsonb.c#L128">pg source code</a>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/json.c#L136">pg source code</a>
    /// </summary>
    public static T DecodeBytes(ref PgBinaryValue value)
    {
        return DecodeBytes(ref value, null);
    }

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Decode a value of type <typeparamref name="T"/> from the binary value as JSON. If the
    /// column's metadata says the type is actually JSONB, read the JSONB version number from the
    /// buffer and ensure it's the expected version code (always 1). The method then passes the
    /// remaining bytes to be decoded by <see cref="Json.FromBytes"/>.
    /// </para>
    /// <para>
    /// This method allows for passing a <see cref="JsonTypeInfo{T}"/> which generally speeds up
    /// deserialization since it knows type information that would otherwise require reflection at
    /// runtime.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/jsonb.c#L128">pg source code</a>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/json.c#L136">pg source code</a>
    /// </summary>
    [SuppressMessage("ReSharper", "InvertIf")]
    public static T DecodeBytes(ref PgBinaryValue value, JsonTypeInfo<T>? typeInfo)
    {
        if (value.ColumnMetadata.PgTypeInfo == PgTypeInfo.Jsonb)
        {
            var versionCode = value.Buffer.ReadByte();
            if (versionCode != JsonBVersion)
            {
                throw ColumnDecodeException.Create<T>(
                    value.ColumnMetadata,
                    $"Unsupported JSONB format version: {versionCode}. Only version 1 is supported");
            }
        }

        return Json.FromBytes(value.Buffer, typeInfo);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Decode a value of type <typeparamref name="T"/> as JSON by deferring to
    /// <see cref="DecodeText(PgTextValue, JsonTypeInfo{T})"/>. This method always uses runtime
    /// JSON serialization which is slower and uses more memory when compared to JSON serialization
    /// with source generation.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/jsonb.c#L112">pg source code</a>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/json.c#L124">pg source code</a>
    /// </summary>
    public static T DecodeText(PgTextValue value)
    {
        return DecodeText(value, null);
    }

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Decode a value of type <typeparamref name="T"/> from the text value as JSON. If the column's
    /// metadata says the type is actually JSONB, read the JSONB version number as the first
    /// character and ensure it's the expected version code (always 1). The method then passes the
    /// remaining chars to be decoded by <see cref="Json.FromChars"/>.
    /// </para>
    /// <para>
    /// This method allows for passing a <see cref="JsonTypeInfo{T}"/> which generally speeds up
    /// deserialization since it knows type information that would otherwise require reflection at
    /// runtime.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/jsonb.c#L128">pg source code</a>
    /// <a href="https://github.com/postgres/postgres/blob/874d817baa160ca7e68bee6ccc9fc1848c56e750/src/backend/utils/adt/json.c#L136">pg source code</a>
    /// </summary>
    public static T DecodeText(PgTextValue value, JsonTypeInfo<T>? typeInfo)
    {
        return Json.FromChars(value.Chars, typeInfo);
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Jsonb;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.JsonbArray;

    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return dbType == DbType || dbType == PgTypeInfo.Json;
    }
}
