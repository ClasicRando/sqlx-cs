using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgBox(PgPoint High, PgPoint Low) : IPgDbType<PgBox>, IPostGisType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"{High.PostGisLiteral},{Low.PostGisLiteral}");

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgBox value, WriteBuffer buffer)
    {
        PgPoint.Encode(value.High, buffer);
        PgPoint.Encode(value.Low, buffer);
    }

    public static PgBox DecodeBytes(PgBinaryValue value)
    {
        return new PgBox(PgPoint.DecodeBytes(value), PgPoint.DecodeBytes(value));
    }

    public static PgBox DecodeText(PgTextValue value)
    {
        var indexPairs = GeometryUtils.ExtractPointIndexes(value);
        if (indexPairs.Count == 2)
        {
            throw ColumnDecodeError.Create<PgBox>(
                value.ColumnMetadata,
                $"Box geoms must have exactly 2 points. Found '{value.Chars}'");
        }

        var (firstPointStart, firstPointEnd) = indexPairs[0];
        PgPoint point1 = PgPoint.DecodeText(value.Slice(firstPointStart..firstPointEnd));
        var (secondPointStart, secondPointEnd) = indexPairs[1];
        PgPoint point2 = PgPoint.DecodeText(value.Slice(secondPointStart..secondPointEnd));
        return new PgBox(point1, point2);
    }
    
    public static PgType DbType => PgType.Box;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgBox value)
    {
        return DbType;
    }
}
