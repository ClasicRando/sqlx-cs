using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Types;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgJson<T> : IPgDbType<T> where T : notnull
{
    public static void Encode(T value, WriteBuffer buffer)
    {
        Encode(value, buffer, null);
    }
    
    public static void Encode(T value, WriteBuffer buffer, JsonTypeInfo<T>? typeInfo)
    {
        buffer.WriteByte(1);
        Json.WriteToStream(buffer, value, typeInfo);
    }

    public static T DecodeBytes(PgBinaryValue value)
    {
        return DecodeBytes(value, null);
    }

    public static T DecodeText(PgTextValue value)
    {
        return DecodeText(value, null);
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public static T DecodeBytes(PgBinaryValue value, JsonTypeInfo<T>? typeInfo)
    {
        if (value.ColumnMetadata.PgType.TypeOid == PgType.Jsonb.TypeOid)
        {
            var versionCode = value.Buffer.ReadByte();
            if (versionCode != 1)
            {
                throw ColumnDecodeError.Create<T>(
                    value.ColumnMetadata,
                    $"Unsupported JSONB format version: {versionCode}. Only version 1 is supported");
            }
        }

        var span = value.Buffer.ReadBytesAsSpan();
        return Json.FromBytes(span, typeInfo);
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public static T DecodeText(PgTextValue value, JsonTypeInfo<T>? typeInfo)
    {
        if (value.ColumnMetadata.PgType.TypeOid == PgType.Jsonb.TypeOid)
        {
            var versionCode = value.Chars[0];
            if (versionCode != 1)
            {
                throw ColumnDecodeError.Create<T>(
                    value.ColumnMetadata,
                    $"Unsupported JSONB format version: {versionCode}. Only version 1 is supported");
            }
        }

        return Json.FromChars(value, typeInfo);
    }
    
    public static PgType DbType => PgType.Jsonb;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid || dbType.TypeOid == PgType.Json.TypeOid;
    }

    public static PgType GetActualType(T value)
    {
        return DbType;
    }
}
