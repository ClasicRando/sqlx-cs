using System.Buffers;
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
public readonly struct PgCircle(PgPoint center, double radius)
    : IPgDbType<PgCircle>, IGeometryType, IHasArrayType, IEquatable<PgCircle>
{
    private readonly Lazy<string> _geometryLiteral = new(() => $"<{center.GeometryLiteral},{radius}>");

    public PgPoint Center { get; } = center;

    public double Radius { get; } = radius;

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the center point using <see cref="PgPoint.Encode"/> followed by the radius
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L4703">pg source code</a>
    /// </summary>
    public static void Encode(PgCircle value, IBufferWriter<byte> buffer)
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
    public static PgCircle DecodeBytes(ref PgBinaryValue value)
    {
        return new PgCircle(PgPoint.DecodeBytes(ref value), value.Buffer.ReadDouble());
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
        var midIndex = value.Chars.IndexOf("),") + 1;
        PgTextValue pointSlice = value.Slice(1..midIndex);
        PgPoint center = GeometryUtils.DecodePoint<PgCircle>(in pointSlice);
        if (!double.TryParse(value.Chars[(midIndex + 1)..^1], out var radius))
        {
            throw ColumnDecodeException.Create<PgCircle>(
                value.ColumnMetadata,
                $"Could not parse radius from '{value.Chars}'");
        }
        return new PgCircle(center, radius);
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Circle;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.CircleArray;

    public static bool IsCompatible(PgTypeInfo dbType)
    {
        return dbType == DbType;
    }

    public bool Equals(PgCircle other)
    {
        return Center.Equals(other.Center) && Radius.Equals(other.Radius);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgCircle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Center, Radius);
    }
    
    public static bool operator ==(PgCircle left, PgCircle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgCircle left, PgCircle right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"PgCircle {{ Center = {Center}, Radius = {Radius} }}";
    }
}
