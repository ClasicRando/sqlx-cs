using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>LSEG</c> type represented as a pair of <see cref="PgPoint"/>s
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-LSEG">docs</a>
/// </summary>
public readonly struct PgLineSegment(PgPoint point1, PgPoint point2)
    : IPgDbType<PgLineSegment>, IGeometryType, IHasArrayType, IEquatable<PgLineSegment>
{
    private readonly Lazy<string> _geometryLiteral = new(() => $"({point1.GeometryLiteral},{point2.GeometryLiteral})");

    public PgPoint Point1 { get; } = point1;

    public PgPoint Point2 { get; } = point2;

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Encodes the 2 <see cref="PgPoint"/>s to the buffer
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L2092">pg source code</a>
    /// </summary>
    public static void Encode(PgLineSegment value, IBufferWriter<byte> buffer)
    {
        PgPoint.Encode(value.Point1, buffer);
        PgPoint.Encode(value.Point2, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Decodes 2 <see cref="PgPoint"/>s from the buffer to create a <see cref="PgLineSegment"/>
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L2111">pg source code</a>
    /// </summary>
    public static PgLineSegment DecodeBytes(ref PgBinaryValue value)
    {
        return new PgLineSegment(PgPoint.DecodeBytes(ref value), PgPoint.DecodeBytes(ref value));
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Extracts 2 <see cref="PgPoint"/>s as the 2 points that define the bounds of the line
    /// segment. The format of the string literal is <c>[{point1},{point2}]</c> so the characters
    /// are sliced to ignore the enclosing brackets and split by the inner comma to pass 2 slices to
    /// <see cref="PgPoint.DecodeText"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L2081">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the inner characters are not 2 points or either point cannot be parsed
    /// </exception>
    public static PgLineSegment DecodeText(in PgTextValue value)
    {
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = GeometryUtils.ExtractPointRanges(pointChars.Chars);
        if (indexPairs.Length != 2)
        {
            throw ColumnDecodeException.Create<PgLineSegment>(
                value.ColumnMetadata,
                $"Line segments must have exactly 2 points. Found '{value.Chars}'");
        }

        PgTextValue slice1 = pointChars.Slice(indexPairs[0]);
        PgPoint point1 = GeometryUtils.DecodePoint<PgLineSegment>(slice1);
        PgTextValue slice2 = pointChars.Slice(indexPairs[1]);
        PgPoint point2 = GeometryUtils.DecodePoint<PgLineSegment>(slice2);
        return new PgLineSegment(point1, point2);
    }
    
    public static PgTypeInfo DbType => PgTypeInfo.Lseg;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.LsegArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    public bool Equals(PgLineSegment other)
    {
        return Point1.Equals(other.Point1) && Point2.Equals(other.Point2);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgLineSegment other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Point1, Point2);
    }
    
    public static bool operator ==(PgLineSegment left, PgLineSegment right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgLineSegment left, PgLineSegment right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{nameof(PgLineSegment)} {{ {nameof(Point1)} = {Point1}, {nameof(Point2)} = {Point2} }}";
    }
}
