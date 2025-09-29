using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>POINT</c> type represented as a pair of coordinates in a two-dimensional space
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-POINTS">docs</a>
/// </summary>
public readonly record struct PgPoint(double X, double Y)
    : IPgDbType<PgPoint>, IGeometryType, IHasArrayType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"({X},{Y})");

    public string GeometryLiteral => _postGisLiteral.Value;

    public static PgPoint operator +(PgPoint p1, PgPoint p2) => new(p1.X + p2.X, p1.Y + p2.Y);

    public static PgPoint operator -(PgPoint p1, PgPoint p2) => new(p1.X - p2.X, p1.Y - p2.Y);

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the x and y coordinates of the point as <see cref="double"/> values
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1853">pg source code</a>
    /// </summary>
    public static void Encode(PgPoint value, WriteBuffer buffer)
    {
        buffer.WriteDouble(value.X);
        buffer.WriteDouble(value.Y);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Extracts 2 <see cref="double"/> values for the <see cref="PgPoint.X"/> and
    /// <see cref="PgPoint.Y"/> coordinates.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1868">pg source code</a>
    /// </summary>
    public static PgPoint DecodeBytes(PgBinaryValue value)
    {
        return new PgPoint(value.Buffer.ReadDouble(), value.Buffer.ReadDouble());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Extracts 2 <see cref="double"/> values for the <see cref="PgPoint.X"/> and
    /// <see cref="PgPoint.Y"/> coordinates from the characters assuming the format is '({x},{y})'
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1842">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If either coordinate cannot be parsed from the characters
    /// </exception>
    public static PgPoint DecodeText(PgTextValue value)
    {
        var commaIndex = value.Chars.IndexOf(',');
        if (!double.TryParse(value.Chars[1..commaIndex], out var x))
        {
            throw ColumnDecodeException.Create<PgPoint>(value.ColumnMetadata, "Could not parse X coordinate");
        }
        return !double.TryParse(value.Chars.Slice(commaIndex + 1, value.Chars.Length - commaIndex - 2), out var y)
            ? throw ColumnDecodeException.Create<PgPoint>(value.ColumnMetadata, "Could not parse Y coordinate")
            : new PgPoint(x, y);
    }
    
    public static PgType DbType => PgType.Point;

    public static PgType ArrayDbType => PgType.PointArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgPoint value)
    {
        return DbType;
    }
}
