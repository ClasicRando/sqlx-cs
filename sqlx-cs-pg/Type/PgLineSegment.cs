using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgLineSegment(PgPoint Point1, PgPoint Point2) : IPgDbType<PgLineSegment>, IPostGisType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"({Point1.PostGisLiteral},{Point2.PostGisLiteral})");

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgLineSegment value, WriteBuffer buffer)
    {
        PgPoint.Encode(value.Point1, buffer);
        PgPoint.Encode(value.Point2, buffer);
    }

    public static PgLineSegment DecodeBytes(PgBinaryValue value)
    {
        return new PgLineSegment(PgPoint.DecodeBytes(value), PgPoint.DecodeBytes(value));
    }

    public static PgLineSegment DecodeText(PgTextValue value)
    {
        PgTextValue pointChars = value.Slice(1..^1);
        var indexPairs = GeometryUtils.ExtractPointIndexes(pointChars);
        if (indexPairs.Count == 2)
        {
            throw ColumnDecodeError.Create<PgLineSegment>(
                value.ColumnMetadata,
                $"Line segments must have exactly 2 points. Found '{value.Chars}'");
        }

        var (firstPointStart, firstPointEnd) = indexPairs[0];
        PgPoint point1 = PgPoint.DecodeText(pointChars.Slice(firstPointStart..firstPointEnd));
        var (secondPointStart, secondPointEnd) = indexPairs[1];
        PgPoint point2 = PgPoint.DecodeText(pointChars.Slice(secondPointStart..secondPointEnd));
        return new PgLineSegment(point1, point2);
    }
    
    public static PgType DbType => PgType.Lseg;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgLineSegment value)
    {
        return DbType;
    }
}
