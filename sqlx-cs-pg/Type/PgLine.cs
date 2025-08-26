using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public readonly record struct PgLine(double A, double B, double C) : IPgDbType<PgLine>, IPostGisType
{
    private readonly Lazy<string> _postGisLiteral = new(() => $"{{{A},{B},{C}}}");

    public string PostGisLiteral => _postGisLiteral.Value;

    public static void Encode(PgLine value, WriteBuffer buffer)
    {
        buffer.WriteDouble(value.A);
        buffer.WriteDouble(value.B);
        buffer.WriteDouble(value.C);
    }

    public static PgLine DecodeBytes(PgBinaryValue value)
    {
        return new PgLine(
            value.Buffer.ReadDouble(),
            value.Buffer.ReadDouble(),
            value.Buffer.ReadDouble());
    }

    public static PgLine DecodeText(PgTextValue value)
    {
        var commaIndex = value.Chars.IndexOf(',');
        var firstPointSpan = value.Chars.Slice(1, commaIndex - 1);
        if (!double.TryParse(firstPointSpan, out var a))
        {
            throw ColumnDecodeError.Create<PgLine>(
                value.ColumnMetadata,
                "Could not parse A value");
        }

        var secondCommaIndex = value.Chars.LastIndexOf(',');
        var secondPointSpan = value.Chars.Slice(commaIndex + 1, secondCommaIndex - commaIndex - 1);
        if (!double.TryParse(secondPointSpan, out var b))
        {
            throw ColumnDecodeError.Create<PgLine>(
                value.ColumnMetadata,
                "Could not parse B value");
        }

        var thirdPointSpan = value.Chars.Slice(
            secondCommaIndex + 1, 
            value.Chars.Length - secondCommaIndex - 2);
        if (!double.TryParse(thirdPointSpan, out var c))
        {
            throw ColumnDecodeError.Create<PgLine>(
                value.ColumnMetadata,
                "Could not parse C value");
        }

        return new PgLine(a, b, c);
    }

    public static PgType DbType => PgType.Line;

    public static bool IsCompatible(PgType dbType)
    {
        return dbType.TypeOid == DbType.TypeOid;
    }

    public static PgType GetActualType(PgLine value)
    {
        return PgType.Line;
    }
}
