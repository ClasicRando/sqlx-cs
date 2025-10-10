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
public readonly record struct PgLineSegment(PgPoint Point1, PgPoint Point2)
    : IPgDbType<PgLineSegment>, IGeometryType, IHasArrayType
{
    private readonly Lazy<string> _geometryLiteral = new(() => $"({Point1.GeometryLiteral},{Point2.GeometryLiteral})");

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Encodes the 2 <see cref="PgPoint"/>s to the buffer
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L2092">pg source code</a>
    /// </summary>
    public static void Encode(PgLineSegment value, WriteBuffer buffer)
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
    public static PgLineSegment DecodeBytes(PgBinaryValue value)
    {
        return new PgLineSegment(PgPoint.DecodeBytes(value), PgPoint.DecodeBytes(value));
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
    public static PgLineSegment DecodeText(PgTextValue value)
    {
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = GeometryUtils.ExtractPointRanges(pointChars);
        if (indexPairs.Length == 2)
        {
            throw ColumnDecodeException.Create<PgLineSegment>(
                value.ColumnMetadata,
                $"Line segments must have exactly 2 points. Found '{value.Chars}'");
        }

        PgPoint point1 = PgPoint.DecodeText(pointChars.Slice(indexPairs[0]));
        PgPoint point2 = PgPoint.DecodeText(pointChars.Slice(indexPairs[1]));
        return new PgLineSegment(point1, point2);
    }
    
    public static PgType DbType => PgType.Lseg;

    public static PgType ArrayDbType => PgType.LsegArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgLineSegment value)
    {
        return DbType;
    }
}
