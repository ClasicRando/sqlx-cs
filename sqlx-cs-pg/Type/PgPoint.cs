using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgPoint(double X, double Y) : IPgDbType<PgPoint>, IPostGisType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"({X},{Y})");

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgPoint value, WriteBuffer buffer)
    {
        buffer.WriteDouble(value.X);
        buffer.WriteDouble(value.Y);
    }

    public static PgPoint DecodeBytes(PgBinaryValue value)
    {
        return new PgPoint(value.Buffer.ReadDouble(), value.Buffer.ReadDouble());
    }

    public static PgPoint DecodeText(PgTextValue value)
    {
        var commaIndex = value.Chars.IndexOf(',');
        if (!double.TryParse(value.Chars.Slice(1, commaIndex - 1), out var x))
        {
            throw ColumnDecodeError.Create<PgPoint>(value.ColumnMetadata, "Could not parse X coordinate");
        }
        if (!double.TryParse(value.Chars.Slice(commaIndex + 1, value.Chars.Length - commaIndex - 2), out var y))
        {
            throw ColumnDecodeError.Create<PgPoint>(value.ColumnMetadata, "Could not parse Y coordinate");
        }

        return new PgPoint(x, y);
    }
    
    public static PgType DbType => PgType.Point;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgPoint value)
    {
        return DbType;
    }
}
