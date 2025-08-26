using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal abstract class PgTime : IPgDbType<TimeOnly>
{
    public static void Encode(TimeOnly value, WriteBuffer buffer)
    {
        buffer.WriteLong((long)value.ToTimeSpan().TotalMicroseconds);
    }

    public static TimeOnly DecodeBytes(PgBinaryValue value)
    {
        var microSeconds = value.Buffer.ReadLong();
        return new TimeOnly(microSeconds * TimeSpan.TicksPerMicrosecond);
    }

    public static TimeOnly DecodeText(PgTextValue value)
    {
        if (!TimeOnly.TryParse(value, null, out TimeOnly time))
        {
            throw ColumnDecodeError.Create<TimeOnly>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a time value");
        }
        
        return time;
    }
    
    public static PgType DbType => PgType.Text;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid
               || dbType.TypeOid == PgType.Varchar.TypeOid
               || dbType.TypeOid == PgType.Xml.TypeOid
               || dbType.TypeOid == PgType.Name.TypeOid
               || dbType.TypeOid == PgType.Bpchar.TypeOid;
    }

    public static PgType GetActualType(TimeOnly value)
    {
        return DbType;
    }
}
