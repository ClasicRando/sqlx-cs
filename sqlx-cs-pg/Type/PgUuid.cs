using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public abstract class PgUuid : IPgDbType<Guid>
{
    public static void Encode(Guid value, WriteBuffer buffer)
    {
        var span = buffer.WriteToSpan(16);
        if (!value.TryWriteBytes(span))
        {
            throw new PgException("Failed to write Guid bytes to buffer");
        }
    }

    public static Guid DecodeBytes(PgBinaryValue value)
    {
        return new Guid(value.Buffer.ReadBytesAsSpan());
    }

    public static Guid DecodeText(PgTextValue value)
    {
        if (!Guid.TryParse(value, null, out Guid guid))
        {
            throw ColumnDecodeError.Create<Guid>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a Guid");
        }

        return guid;
    }

    public static PgType DbType => PgType.Uuid;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(Guid value)
    {
        return DbType;
    }
}
