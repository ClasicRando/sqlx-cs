using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public abstract class PgUuid : IPgDbType<Guid>, IHasArrayType
{
    public static void Encode(Guid value, WriteBuffer buffer)
    {
        var span = buffer.GetSpan(16);
        if (!value.TryWriteBytes(span))
        {
            throw ColumnEncodeException.Create<Guid>(DbType, "Failed to write Guid bytes to buffer");
        }
        buffer.Advance(16);
    }

    public static Guid DecodeBytes(PgBinaryValue value)
    {
        return new Guid(value.Buffer.ReadBytesAsSpan());
    }

    public static Guid DecodeText(PgTextValue value)
    {
        if (!Guid.TryParse(value, null, out Guid guid))
        {
            throw ColumnDecodeException.Create<Guid>(
                value.ColumnMetadata,
                $"Could not parse '{value}' into a Guid");
        }

        return guid;
    }

    public static PgType DbType => PgType.Uuid;

    public static PgType ArrayDbType => PgType.UuidArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(Guid value)
    {
        return DbType;
    }
}
