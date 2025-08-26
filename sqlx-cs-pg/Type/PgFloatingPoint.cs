using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal static class PgFloatingPoint
{
    public static double ExtractFloat<T>(this PgBinaryValue value) where T : notnull
    {
        return value.Buffer.Remaining switch
        {
            4 => value.Buffer.ReadFloat(),
            8 => value.Buffer.ReadDouble(),
            _ => throw ColumnDecodeError.Create<T>(
                value.ColumnMetadata,
                $"Could not extract float from buffer. Number of bytes = {value.Buffer.Remaining}"),
        };
    }
    
    public static double ExtractFloat<T>(this PgTextValue value) where T : notnull
    {
        if (!double.TryParse(value, null, out var parseResult))
        {
            throw ColumnDecodeError.Create<T>(
                value.ColumnMetadata,
                $"Could not convert {value} into {typeof(T)}");
        }
        return parseResult;
    }
    
    public static bool IsFloatCompatible(PgType dbType)
    {
        return dbType.TypeOid == PgType.Float4.TypeOid
               || dbType.TypeOid == PgType.Float8.TypeOid;
    }
}

internal abstract class PgDouble : IPgDbType<double>
{
    public static void Encode(double value, WriteBuffer buffer)
    {
        buffer.WriteDouble(value);
    }

    public static double DecodeBytes(PgBinaryValue value)
    {
        return value.ExtractFloat<double>();
    }

    public static double DecodeText(PgTextValue value)
    {
        return value.ExtractFloat<double>();
    }

    public static PgType DbType => PgType.Int8;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgFloatingPoint.IsFloatCompatible(dbType);
    }

    public static PgType GetActualType(double value)
    {
        return DbType;
    }
}

internal abstract class PgFloat : IPgDbType<float>
{
    public static void Encode(float value, WriteBuffer buffer)
    {
        buffer.WriteFloat(value);
    }

    public static float DecodeBytes(PgBinaryValue value)
    {
        return ValidateFloat(value.ExtractFloat<float>(), value.ColumnMetadata);
    }

    public static float DecodeText(PgTextValue value)
    {
        return ValidateFloat(value.ExtractFloat<float>(), value.ColumnMetadata);
    }

    private static float ValidateFloat(double floatingPoint, PgColumnMetadata columnMetadata)
    {
        if (floatingPoint is < float.MinValue or > float.MaxValue)
        {
            throw ColumnDecodeError.Create<float>(
                columnMetadata,
                "Floating point value is outside the bounds of float");
        }
        return (float)floatingPoint;
    }

    public static PgType DbType => PgType.Float4;
    
    public static bool IsCompatible(PgType dbType)
    {
        return PgFloatingPoint.IsFloatCompatible(dbType);
    }

    public static PgType GetActualType(float value)
    {
        return DbType;
    }
}
