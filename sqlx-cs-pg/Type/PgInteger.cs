using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Core.Types;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal static class PgInteger
{
    public static long ExtractInteger<T>(this PgBinaryValue value) where T : notnull
    {
        return value.Buffer.Remaining switch
        {
            2 => value.Buffer.ReadShort(),
            4 => value.Buffer.ReadInt(),
            8 => value.Buffer.ReadLong(),
            _ => throw ColumnDecodeError.Create<T>(
                value.ColumnMetadata,
                $"Could not extract integer from buffer. Number of bytes = {value.Buffer.Remaining}"),
        };
    }
    
    public static long ExtractInteger<T>(this PgTextValue value) where T : notnull
    {
        if (!long.TryParse(value, null, out var parseResult))
        {
            throw ColumnDecodeError.Create<T>(
                value.ColumnMetadata,
                $"Could not convert {value} into {typeof(T)}");
        }
        return parseResult;
    }
    
    public static bool IsIntegerCompatible(PgType dbType)
    {
        return dbType.TypeOid == PgType.Int8.TypeOid
               || dbType.TypeOid == PgType.Int4.TypeOid
               || dbType.TypeOid == PgType.Int2.TypeOid;
    }
}

internal abstract class PgLong : IPgDbType<long>
{
    public static void Encode(long value, WriteBuffer buffer)
    {
        buffer.WriteLong(value);
    }

    public static long DecodeBytes(PgBinaryValue value)
    {
        return value.ExtractInteger<long>();
    }

    public static long DecodeText(PgTextValue value)
    {
        return value.ExtractInteger<long>();
    }

    public static PgType DbType => PgType.Int8;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgInteger.IsIntegerCompatible(dbType);
    }

    public static PgType GetActualType(long value)
    {
        return DbType;
    }
}

internal abstract class PgInt : IPgDbType<int>
{
    public static void Encode(int value, WriteBuffer buffer)
    {
        buffer.WriteInt(value);
    }

    public static int DecodeBytes(PgBinaryValue value)
    {
        var integer = value.ExtractInteger<int>();
        return Integers.ValidateInt(integer, value.ColumnMetadata);
    }

    public static int DecodeText(PgTextValue value)
    {
        var integer = value.ExtractInteger<int>();
        return Integers.ValidateInt(integer, value.ColumnMetadata);
    }

    public static PgType DbType => PgType.Int4;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgInteger.IsIntegerCompatible(dbType);
    }

    public static PgType GetActualType(int value)
    {
        return DbType;
    }
}

internal abstract class PgShort : IPgDbType<short>
{
    public static void Encode(short value, WriteBuffer buffer)
    {
        buffer.WriteShort(value);
    }

    public static short DecodeBytes(PgBinaryValue value)
    {
        var integer = value.ExtractInteger<int>();
        return Integers.ValidateShort(integer, value.ColumnMetadata);
    }

    public static short DecodeText(PgTextValue value)
    {
        var integer = value.ExtractInteger<int>();
        return Integers.ValidateShort(integer, value.ColumnMetadata);
    }

    public static PgType DbType => PgType.Int2;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgInteger.IsIntegerCompatible(dbType);
    }

    public static PgType GetActualType(short value)
    {
        return DbType;
    }
}
