using System.Buffers;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>Postgres <c>BOX</c> type represented as a pair of <see cref="PgPoint"/></para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-BOXES">docs</a>
/// </summary>
public readonly struct PgBox : IPgDbType<PgBox>, IGeometryType, IHasArrayType, IEquatable<PgBox>
{
    public PgPoint High { get; internal init; }

    public PgPoint Low { get; internal init; }

    public string GeometryLiteral => $"{High.GeometryLiteral},{Low.GeometryLiteral}";

    /// <summary>
    /// Create a new box using the 2 points provided. These points are just used as reference since
    /// there is no guarantee that the points with truly represent the opposite corners of the box.
    /// This constructor will construct new points to ensure the <see cref="High"/> and
    /// <see cref="Low"/> properties are correct.
    /// </summary>
    /// <param name="point1">First point of the box</param>
    /// <param name="point2">Second point of the box</param>
    public PgBox(in PgPoint point1, in PgPoint point2)
    {
        High = new PgPoint(double.Max(point1.X, point2.X), double.Max(point1.Y, point2.Y));
        Low = new PgPoint(double.Min(point1.X, point2.X), double.Min(point1.Y, point2.Y));
    }

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes both <see cref="PgPoint"/>s to the argument buffer
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L466">pg source code</a>
    /// </summary>
    public static void Encode(PgBox value, IBufferWriter<byte> buffer)
    {
        PgPoint.Encode(value.High, buffer);
        PgPoint.Encode(value.Low, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Extracts 2 <see cref="PgPoint"/>s that define the bounds of the box
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L501">pg source code</a>
    /// </summary>
    public static PgBox DecodeBytes(ref PgBinaryValue value)
    {
        return new PgBox
        {
            High = PgPoint.DecodeBytes(ref value),
            Low = PgPoint.DecodeBytes(ref value),
        };
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Extracts 2 <see cref="PgPoint"/>s that define the bounds of the box. The format of the
    /// string literal is '({point1}),({point2})' so we split by the comma that separates the 2
    /// points and pass each point to <see cref="PgPoint.DecodeText"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L455">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If 2 points cannot be found within the characters or the points cannot be parsed
    /// </exception>
    public static PgBox DecodeText(PgTextValue value)
    {
        var indexPairs = GeometryUtils.ExtractPointRanges(value.Chars);
        if (indexPairs.Length != 2)
        {
            throw ColumnDecodeException.Create<PgBox>(
                value.ColumnMetadata,
                $"Box geoms must have exactly 2 points. Found {indexPairs.Length} in '{value.Chars}'");
        }

        PgTextValue slice1 = value.Slice(indexPairs[0]);
        PgPoint point1 = GeometryUtils.DecodePoint<PgBox>(slice1);
        PgTextValue slice2 = value.Slice(indexPairs[1]);
        PgPoint point2 = GeometryUtils.DecodePoint<PgBox>(slice2);
        return new PgBox
        {
            High = point1,
            Low = point2,
        };
    }

    public static PgTypeInfo DbType => PgTypeInfo.Box;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.BoxArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    public bool Equals(PgBox other)
    {
        return High.Equals(other.High) && Low.Equals(other.Low);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgBox other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(High, Low);
    }

    public static bool operator ==(PgBox left, PgBox right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgBox left, PgBox right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{nameof(PgBox)} {{ {nameof(High)} = {High}, {nameof(Low)} {Low} }}";
    }
}
