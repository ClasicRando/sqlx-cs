using System.Globalization;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public abstract class PgDateTime : IPgDbType<DateTime>
{
    private const long PostgresEpochSeconds = 946_684_800;
    private const long PostgresEpochTicks = PostgresEpochSeconds * TimeSpan.TicksPerSecond;
    
    public static void Encode(DateTime value, WriteBuffer buffer)
    {
        buffer.WriteLong((value.Ticks - PostgresEpochTicks) / TimeSpan.TicksPerMicrosecond);
    }

    public static DateTime DecodeBytes(PgBinaryValue value)
    {
        return new DateTime(value.Buffer.ReadLong() * TimeSpan.TicksPerMicrosecond + PostgresEpochTicks);
    }

    public static DateTime DecodeText(PgTextValue value)
    {
        if (DateTime.TryParse(value, null, DateTimeStyles.RoundtripKind, out DateTime dateTime))
        {
            return dateTime;
        }
        
        throw ColumnDecodeError.Create<DateTime>(
            value.ColumnMetadata,
            $"Cannot parse '{value}' as a DateTime");
    }
    
    public static PgType DbType => PgType.Timestamp;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == PgType.Timestamp.TypeOid
               || dbType.TypeOid == PgType.Timestamptz.TypeOid;
    }

    public static PgType GetActualType(DateTime value)
    {
        return DbType;
    }
}
