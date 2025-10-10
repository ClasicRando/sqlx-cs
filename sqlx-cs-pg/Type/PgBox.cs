using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>Postgres <c>BOX</c> type represented as a pair of points</para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-BOXES">docs</a>
/// </summary>
public readonly record struct PgBox(PgPoint High, PgPoint Low)
    : IPgDbType<PgBox>, IGeometryType, IHasArrayType
{
    private readonly Lazy<string> _geometryLiteral = new(() => $"{High.GeometryLiteral},{Low.GeometryLiteral}");

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes both <see cref="PgPoint"/>s to the argument buffer
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L466">pg source code</a>
    /// </summary>
    public static void Encode(PgBox value, WriteBuffer buffer)
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
    public static PgBox DecodeBytes(PgBinaryValue value)
    {
        return new PgBox(PgPoint.DecodeBytes(value), PgPoint.DecodeBytes(value));
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
        var indexPairs = GeometryUtils.ExtractPointRanges(value);
        if (indexPairs.Length == 2)
        {
            throw ColumnDecodeException.Create<PgBox>(
                value.ColumnMetadata,
                $"Box geoms must have exactly 2 points. Found '{value.Chars}'");
        }

        PgPoint point1 = PgPoint.DecodeText(value.Slice(indexPairs[0]));
        PgPoint point2 = PgPoint.DecodeText(value.Slice(indexPairs[1]));
        return new PgBox(point1, point2);
    }
    
    public static PgType DbType => PgType.Box;

    public static PgType ArrayDbType => PgType.BoxArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgBox value)
    {
        return DbType;
    }
}
