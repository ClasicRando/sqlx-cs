using System.Text;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgPolygon(PgPoint[] Points) : IPgDbType<PgPolygon>, IPostGisType
{
    private readonly Lazy<PgBox> _boundingBox = new(() => MakeBoundingBox(Points));

    public PgBox BoundingBox => _boundingBox.Value;
    
    private readonly Lazy<string> _postGisLiteral = new(() =>
    {
        var builder = new StringBuilder();
        builder.Append('(');
        for (var i = 0; i < Points.Length; i++)
        {
            PgPoint point = Points[i];
            if (i > 0)
            {
                builder.Append(',');
            }
            builder.Append(point.PostGisLiteral);
        }
        builder.Append(')');
        return builder.ToString();
    });

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgPolygon value, WriteBuffer buffer)
    {
        buffer.WriteInt(value.Points.Length);
        foreach (PgPoint point in value.Points)
        {
            PgPoint.Encode(point, buffer);
        }
    }

    public static PgPolygon DecodeBytes(PgBinaryValue value)
    {
        var size = value.Buffer.ReadInt();
        var points = new PgPoint[size];
        for (var i = 0; i < size; i++)
        {
            points[i] = PgPoint.DecodeBytes(value);
        }
        return new PgPolygon(points);
    }

    public static PgPolygon DecodeText(PgTextValue value)
    {
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = GeometryUtils.ExtractPointIndexes(pointChars);
        var points = new PgPoint[indexPairs.Count];
        for (var i = 0; i < points.Length; i++)
        {
            var (pointStart, pointEnd) = indexPairs[i];
            points[i] = PgPoint.DecodeText(pointChars.Slice(pointStart..pointEnd));
        }
        return new PgPolygon(points);
    }
    
    public static PgType DbType => PgType.Polygon;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgPolygon value)
    {
        return DbType;
    }

    private static PgBox MakeBoundingBox(PgPoint[] points)
    {
        if (points.Length == 0)
        {
            throw new ArgumentException("Cannot make a bounding box with 0 points", nameof(points));
        }

        var x1 = points[0].X;
        var x2 = points[0].X;
        var y1 = points[0].Y;
        var y2 = points[0].Y;
        foreach (PgPoint point in points[1..])
        {
            if (point.X < x1)
            {
                x1 = point.X;
            }
            if (point.X > x2)
            {
                x2 = point.X;
            }
            if (point.Y < y1)
            {
                y1 = point.Y;
            }
            if (point.Y > y2)
            {
                y2 = point.Y;
            }
        }

        return new PgBox(new PgPoint(x2, y2), new PgPoint(x1, y1));
    }
}
