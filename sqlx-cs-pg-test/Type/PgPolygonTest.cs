using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPolygon))]
public class PgPolygonTest
{
    [Theory]
    [InlineData(
        new[] { 5.63, 8.59, 4.87, 2.8 },
        new byte[]
        {
            0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64,
            19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        })]
    [InlineData(
        new[] { 4.87, 2.8 },
        new byte[]
        {
            0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        })]
    public void Encode_Should_WritePolygon(
        double[] values,
        byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var value = new PgPolygon(points);

        PgPolygon.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[]
        {
            0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64,
            19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        },
        new[] { 5.63, 8.59, 4.87, 2.8 })]
    [InlineData(
        new byte[]
        {
            0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        },
        new[] { 4.87, 2.8 })]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsPolygon(
        byte[] binaryData,
        double[] values)
    {
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var expectedValue = new PgPolygon(points);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgPolygon actualValue = PgPolygon.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(
        "((5.63,8.59),(4.87,2.8))",
        new[] { 5.63, 8.59, 4.87, 2.8 })]
    [InlineData(
        "((5.63,8.59))",
        new[] { 5.63, 8.59 })]
    public void DecodeText_Should_DecodeTextEncodedValueAsPolygon(
        string textData,
        double[] values)
    {
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var expectedValue = new PgPolygon(points);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgPolygon actualValue = PgPolygon.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DbType_Should_ReturnPolygonType() => Assert.Equal(PgPolygon.DbType, PgTypeInfo.Polygon);

    [Fact]
    public void ArrayDbType_Should_ReturnPolygonType() =>
        Assert.Equal(PgPolygon.ArrayDbType, PgTypeInfo.PolygonArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgPolygon.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Polygon, true];
        yield return [PgTypeInfo.PolygonArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
