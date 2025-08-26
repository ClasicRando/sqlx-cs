using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgCircle(PgPoint Center, double Radius) : IPgDbType<PgCircle>, IPostGisType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"<{Center.PostGisLiteral},{Radius}>");

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgCircle value, WriteBuffer buffer)
    {
        PgPoint.Encode(value.Center, buffer);
        buffer.WriteDouble(value.Radius);
    }

    public static PgCircle DecodeBytes(PgBinaryValue value)
    {
        return new PgCircle(PgPoint.DecodeBytes(value), value.Buffer.ReadDouble());
    }

    public static PgCircle DecodeText(PgTextValue value)
    {
        var midIndex = value.Chars.IndexOf("),");
        PgPoint center = PgPoint.DecodeText(value.Slice(1..midIndex));
        if (!double.TryParse(value.Chars[midIndex..^1], out var radius))
        {
            throw ColumnDecodeError.Create<PgCircle>(
                value.ColumnMetadata,
                $"Could not parse radius from '{value.Chars}'");
        }
        return new PgCircle(center, radius);
    }
    
    public static PgType DbType => PgType.Circle;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgCircle value)
    {
        return DbType;
    }
}
