using Sqlx.Core.Buffer;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

/// <summary>
/// <para>
/// Postgres <c>PATH</c> type represented as a collection of points that may or may not be closed
/// </para>
/// <a href="https://www.postgresql.org/docs/current/datatype-geometric.html#DATATYPE-GEOMETRIC-PATHS">docs</a>
/// </summary>
public readonly record struct PgPath(bool IsClosed, PgPoint[] Points)
    : IPgDbType<PgPath>, IGeometryType, IHasArrayType
{
    private readonly Lazy<string> _geometryLiteral = new(
        () => GeometryUtils.GeneratePointCollectionLiteral(Points, IsClosed));

    public string GeometryLiteral => _geometryLiteral.Value;

    /// <inheritdoc cref="IPgDbType{T}.Encode"/>
    /// <summary>
    /// <para>
    /// Writes a 1/0 byte (1 = is closed path, 0 = is open path) followed by the points encoded
    /// using <see cref="GeometryUtils.EncodePoints"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1488">pg source code</a>
    /// </summary>
    public static void Encode(PgPath value, WriteBuffer buffer)
    {
        buffer.WriteByte((byte)(value.IsClosed ? 1 : 0));
        GeometryUtils.EncodePoints(value.Points, buffer);
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeBytes"/>
    /// <summary>
    /// <para>
    /// Reads the first byte in the buffer to figure out if the path is closed or open. Then reads
    /// all points using <see cref="GeometryUtils.DecodePoints(PgBinaryValue)"/>.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1526">pg source code</a>
    /// </summary>
    public static PgPath DecodeBytes(PgBinaryValue value)
    {
        var isClosed = value.Buffer.ReadByte() == 1;
        return new PgPath(isClosed, GeometryUtils.DecodePoints(value));
    }

    /// <inheritdoc cref="IPgDbType{T}.DecodeText"/>
    /// <summary>
    /// <para>
    /// Uses the first character to decide if the path is closed or open, then extracts all points
    /// from the characters using <see cref="GeometryUtils.DecodePoints(PgTextValue)"/>. The format
    /// is assumed to be <c>((x1,y1),...(xn,yn))</c> for closed paths and
    /// <c>[(x1,y1),...(xn,yn)]</c> for open paths.
    /// </para>
    /// <a href="https://github.com/postgres/postgres/blob/1fe66680c09b6cc1ed20236c84f0913a7b786bbc/src/backend/utils/adt/geo_ops.c#L1474">pg source code</a>
    /// </summary>
    /// <exception cref="Sqlx.Core.Exceptions.ColumnDecodeException">
    /// If characters do not represent a collection of points
    /// </exception>
    public static PgPath DecodeText(PgTextValue value)
    {
        var isClosed = value.Chars[0] == '(';
        return new PgPath(isClosed, GeometryUtils.DecodePoints(value));
    }
    
    public static PgType DbType => PgType.Path;

    public static PgType ArrayDbType => PgType.PathArray;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgPath value)
    {
        return DbType;
    }
}
