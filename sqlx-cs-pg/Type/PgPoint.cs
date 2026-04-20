using System.Buffers;
using System.Runtime.CompilerServices;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>POINT</c> type represented as a pair of coordinates in a two-dimensional space
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-POINTS">docs</a>
/// </summary>
public readonly struct PgPoint(double x, double y)
    : IPgDbType<PgPoint>, IGeometryType, IHasArrayType, IEquatable<PgPoint>
{
    internal const int Size = sizeof(double) + sizeof(double);
    private readonly Lazy<string> _geometryLiteral = new(() => $"({x},{y})");

    public double X { get; } = x;

    public double Y { get; } = y;

    public string GeometryLiteral => _geometryLiteral.Value;

    public static PgPoint operator +(PgPoint p1, PgPoint p2) => p1.Add(p2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PgPoint Add(PgPoint other)
    {
        return new PgPoint(X + other.X, Y + other.Y);
    }

    public static PgPoint operator -(PgPoint p1, PgPoint p2) => p1.Subtract(p2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PgPoint Subtract(PgPoint other)
    {
        return new PgPoint(X - other.X, Y - other.Y);
    }

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes the x and y coordinates of the point as <see cref="double"/> values
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1853">pg source code</a>
    /// </summary>
    public static void Encode(PgPoint value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
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
    public static PgPoint DecodeBytes(in PgBinaryValue value)
    {
        var buff = value.Buffer;
        return new PgPoint(buff.ReadDouble(), buff.ReadDouble());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <seealso cref="GeometryUtils.DecodePoint"/>
    public static PgPoint DecodeText(in PgTextValue value)
    {
        return GeometryUtils.DecodePoint<PgPoint>(value);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Point;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.PointArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    public bool Equals(PgPoint other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgPoint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(PgPoint left, PgPoint right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgPoint left, PgPoint right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"PgPoint {{ {nameof(X)} = {X}, {nameof(Y)} = {Y} }}";
    }
}
