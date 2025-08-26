using System.Text;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgPath(bool IsClosed, PgPoint[] Points) : IPgDbType<PgPath>, IPostGisType
{
    private readonly Lazy<string> _postGisLiteral = new(() =>
    {
        var builder = new StringBuilder();
        builder.Append(IsClosed ? '(' : '[');
        for (var i = 0; i < Points.Length; i++)
        {
            PgPoint point = Points[i];
            if (i > 0)
            {
                builder.Append(',');
            }
            builder.Append(point.PostGisLiteral);
        }
        builder.Append(IsClosed ? ')' : ']');
        return builder.ToString();
    });

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgPath value, WriteBuffer buffer)
    {
        buffer.WriteByte((byte)(value.IsClosed ? 1 : 0));
        buffer.WriteInt(value.Points.Length);
        foreach (PgPoint point in value.Points)
        {
            PgPoint.Encode(point, buffer);
        }
    }

    public static PgPath DecodeBytes(PgBinaryValue value)
    {
        var isClosed = value.Buffer.ReadByte() == 1;
        var size = value.Buffer.ReadInt();
        var points = new PgPoint[size];
        for (var i = 0; i < size; i++)
        {
            points[i] = PgPoint.DecodeBytes(value);
        }
        return new PgPath(isClosed, points);
    }

    public static PgPath DecodeText(PgTextValue value)
    {
        var isClosed = value.Chars[0] == '(';
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = GeometryUtils.ExtractPointIndexes(pointChars);
        var points = new PgPoint[indexPairs.Count];
        for (var i = 0; i < points.Length; i++)
        {
            var (pointStart, pointEnd) = indexPairs[i];
            points[i] = PgPoint.DecodeText(pointChars.Slice(pointStart..pointEnd));
        }
        return new PgPath(isClosed, points);
    }
    
    public static PgType DbType => PgType.Path;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgPath value)
    {
        return DbType;
    }
}
