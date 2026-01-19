using System.Buffers;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>Postgres <c>LINE</c> type represented as a linear equation:</para>
/// <para><see cref="A"/>x + <see cref="B"/>y + <see cref="C"/> = 0</para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-LINE">docs</a>
/// </summary>
public readonly struct PgLine(double a, double b, double c)
    : IPgDbType<PgLine>, IGeometryType, IHasArrayType, IEquatable<PgLine>
{
    private readonly Lazy<string> _geometryLiteral = new(() => $"{{{a},{b},{c}}}");

    public double A { get; } = a;

    public double B { get; } = b;

    public double C { get; } = c;

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes all 3 <see cref="double"/> values in alphabetic order
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1038">pg source code</a>
    /// </summary>
    public static void Encode(PgLine value, IBufferWriter<byte> buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.WriteDouble(value.A);
        buffer.WriteDouble(value.B);
        buffer.WriteDouble(value.C);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Read all 3 <see cref="double"/> values in alphabetic order
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1061">pg source code</a>
    /// </summary>
    public static PgLine DecodeBytes(ref PgBinaryValue value)
    {
        return new PgLine(
            value.Buffer.ReadDouble(),
            value.Buffer.ReadDouble(),
            value.Buffer.ReadDouble());
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Extracts 3 Double values from the characters assuming the format is <c>({a},{b},{c})</c>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1023">pg source code</a>
    /// </summary>
    /// <exception cref="ColumnDecodeException">
    /// If the characters do not match the expected format or any part of the line cannot be
    /// converted to a <see cref="double"/>
    /// </exception>
    public static PgLine DecodeText(in PgTextValue value)
    {
        var commaIndex = value.Chars.IndexOf(',');
        var firstPointSpan = value.Chars[1..commaIndex];
        if (!double.TryParse(firstPointSpan, out var a))
        {
            throw ColumnDecodeException.Create<PgLine>(
                value.ColumnMetadata,
                "Could not parse A value");
        }

        var secondCommaIndex = value.Chars.LastIndexOf(',');
        var secondPointSpan = value.Chars.Slice(commaIndex + 1, secondCommaIndex - commaIndex - 1);
        if (!double.TryParse(secondPointSpan, out var b))
        {
            throw ColumnDecodeException.Create<PgLine>(
                value.ColumnMetadata,
                "Could not parse B value");
        }

        var thirdPointSpan = value.Chars.Slice(
            secondCommaIndex + 1, 
            value.Chars.Length - secondCommaIndex - 2);
        if (!double.TryParse(thirdPointSpan, out var c))
        {
            throw ColumnDecodeException.Create<PgLine>(
                value.ColumnMetadata,
                "Could not parse C value");
        }

        return new PgLine(a, b, c);
    }

    public static PgTypeInfo DbType => PgTypeInfo.Line;

    public static PgTypeInfo ArrayDbType => PgTypeInfo.LineArray;

    public static bool IsCompatible(PgTypeInfo typeInfo)
    {
        return typeInfo == DbType;
    }

    public bool Equals(PgLine other)
    {
        return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
    }

    public override bool Equals(object? obj)
    {
        return obj is PgLine other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, B, C);
    }
    
    public static bool operator ==(PgLine left, PgLine right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PgLine left, PgLine right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{nameof(PgLine)} {{ {nameof(A)} = {A}, {nameof(B)} = {B}, {nameof(C)} = {C} }}";
    }
}
