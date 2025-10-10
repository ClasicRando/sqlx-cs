using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>POLYGON</c> type represented by a list of points as vertices of the polygon 
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-POLYGON">docs</a>
/// </summary>
public readonly record struct PgPolygon(PgPoint[] Points)
    : IPgDbType<PgPolygon>, IGeometryType, IHasArrayType
{
    private readonly Lazy<PgBox> _boundingBox = new(() => MakeBoundingBox(Points));

    /// <summary>
    /// The bounding box of the polygon. This is the smallest box object that fully covers the
    /// polygon area. 
    /// </summary>
    public PgBox BoundingBox => _boundingBox.Value;
    
    private readonly Lazy<string> _geometryLiteral = new(
        () => GeometryUtils.GeneratePointCollectionLiteral(Points, true));

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes all points to the buffer using <see cref="GeometryUtils.EncodePoints"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L3475">pg source code</a>
    /// </summary>
    public static void Encode(PgPolygon value, WriteBuffer buffer)
    {
        GeometryUtils.EncodePoints(value.Points, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Reads all points using <see cref="GeometryUtils.DecodePoints(PgBinaryValue)"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L3510">pg source code</a>
    /// </summary>
    public static PgPolygon DecodeBytes(PgBinaryValue value)
    {
        return new PgPolygon(GeometryUtils.DecodePoints(value));
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// extracts all points from the characters using
    /// <see cref="GeometryUtils.DecodePoints(PgTextValue)"/>. The format is assumed to be
    /// <c>((x1,y1),...(xn,yn))</c>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L3459">pg source code</a>
    /// </summary>
    /// <exception cref="Sqlx.Core.Exceptions.ColumnDecodeException">
    /// If characters do not represent a collection of points
    /// </exception>
    public static PgPolygon DecodeText(PgTextValue value)
    {
        return new PgPolygon(GeometryUtils.DecodePoints(value));
    }
    
    public static PgType DbType => PgType.Polygon;

    public static PgType ArrayDbType => PgType.PolygonArray;

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
