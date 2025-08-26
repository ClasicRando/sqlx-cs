using System.Globalization;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgDate : IPgDbType<DateOnly>
{
    private static readonly DateOnly PostgresEpoch = new(2000, 1, 1);
    
    public static void Encode(DateOnly value, WriteBuffer buffer)
    {
        buffer.WriteInt(value.DayNumber - PostgresEpoch.DayNumber);
    }

    public static DateOnly DecodeBytes(PgBinaryValue value)
    {
        return PostgresEpoch.AddDays(value.Buffer.ReadInt());
    }

    public static DateOnly DecodeText(PgTextValue value)
    {
        if (DateOnly.TryParse(value, null, DateTimeStyles.RoundtripKind, out DateOnly date))
        {
            return date;
        }
        
        throw ColumnDecodeError.Create<DateOnly>(
            value.ColumnMetadata,
            $"Cannot parse '{value}' as a DateOnly");
    }
    
    public static PgType DbType => PgType.Date;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(DateOnly value)
    {
        return DbType;
    }
}
