using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public abstract class PgDateTimeOffset : IPgDbType<DateTimeOffset>
{
    public static void Encode(DateTimeOffset value, WriteBuffer buffer)
    {
        PgDateTime.Encode(value.UtcDateTime, buffer);
    }

    public static DateTimeOffset DecodeBytes(PgBinaryValue value)
    {
        return new DateTimeOffset(PgDateTime.DecodeBytes(value), TimeSpan.Zero);
    }

    public static DateTimeOffset DecodeText(PgTextValue value)
    {
        return new DateTimeOffset(PgDateTime.DecodeText(value), TimeSpan.Zero);
    }
    
    public static PgType DbType => PgType.Timestamptz;

    public static bool IsCompatible(PgType dbType)
    {
        return PgDateTime.IsCompatible(dbType);
    }

    public static PgType GetActualType(DateTimeOffset value)
    {
        return DbType;
    }
}
