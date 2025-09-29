using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>CIRCLE</c> type represented as a center point and a radius
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-CIRCLE">docs</a>
/// </summary>
public readonly record struct PgCircle(PgPoint Center, double Radius)
    : IPgDbType<PgCircle>, IGeometryType, IHasArrayType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"<{Center.GeometryLiteral},{Radius}>");

    public string GeometryLiteral => _postGisLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the center point using <see cref="PgPoint.Encode"/> followed by the radius
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L4703">pg source code</a>
    /// </summary>
    public static void Encode(PgCircle value, WriteBuffer buffer)
    {
        PgPoint.Encode(value.Center, buffer);
        buffer.WriteDouble(value.Radius);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Extracts the center point using <see cref="PgPoint.DecodeBytes"/> followed by the radius
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L4727">pg source code</a>
    /// </summary>
    public static PgCircle DecodeBytes(PgBinaryValue value)
    {
        return new PgCircle(PgPoint.DecodeBytes(value), value.Buffer.ReadDouble());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// The expected format is <c>&lt;(x,y),r&gt;</c> so the point component is extracted and passed
    /// to <see cref="PgPoint.DecodeText"/> while the radius is extracted as a <see cref="double"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L4681">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If parsing the point fails or the radius component is not a double
    /// </exception>
    public static PgCircle DecodeText(PgTextValue value)
    {
        var midIndex = value.Chars.IndexOf("),");
        PgPoint center = PgPoint.DecodeText(value.Slice(1..midIndex));
        if (!double.TryParse(value.Chars[midIndex..^1], out var radius))
        {
            throw ColumnDecodeException.Create<PgCircle>(
                value.ColumnMetadata,
                $"Could not parse radius from '{value.Chars}'");
        }
        return new PgCircle(center, radius);
    }
    
    public static PgType DbType => PgType.Circle;

    public static PgType ArrayDbType => PgType.CircleArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgCircle value)
    {
        return DbType;
    }
}
