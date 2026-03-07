using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPolygon))]
public class PgPolygonTest
{
    [Test]
    [Arguments(
        new[] { 5.63, 8.59, 4.87, 2.8 },
        new byte[]
        {
            0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64,
            19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        })]
    [Arguments(
        new[] { 4.87, 2.8 },
        new byte[]
        {
            0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        })]
    public async Task Encode_Should_WritePolygon(
        double[] values,
        byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var value = new PgPolygon([..points]);

        PgPolygon.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(
        new byte[]
        {
            0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64,
            19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        },
        new[] { 5.63, 8.59, 4.87, 2.8 })]
    [Arguments(
        new byte[]
        {
            0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        },
        new[] { 4.87, 2.8 })]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsPolygon(
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

        var expectedValue = new PgPolygon([..points]);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgPolygon actualValue = PgPolygon.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(
        "((5.63,8.59),(4.87,2.8))",
        new[] { 5.63, 8.59, 4.87, 2.8 })]
    [Arguments(
        "((5.63,8.59))",
        new[] { 5.63, 8.59 })]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsPolygon(
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

        var expectedValue = new PgPolygon([..points]);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgPolygon actualValue = PgPolygon.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DbType_Should_ReturnPolygonType() => await Assert.That(PgTypeInfo.Polygon).IsEqualTo(PgPolygon.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnPolygonType() =>
        await Assert.That(PgTypeInfo.PolygonArray).IsEqualTo(PgPolygon.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgPolygon.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Polygon, true];
        yield return [PgTypeInfo.PolygonArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
