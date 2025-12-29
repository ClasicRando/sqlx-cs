using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBox))]
public class PgBoxTest
{
    [Test]
    [Arguments(
        4,
        3,
        2,
        1,
        new byte[]
        {
            64, 16, 0, 0, 0, 0, 0, 0, 64, 8, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 63, 240, 0,
            0, 0, 0, 0, 0,
        })]
    public async Task Encode_Should_WriteBox(
        double x1,
        double y1,
        double x2,
        double y2,
        byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();
        var value = new PgBox(new PgPoint(x1, y1), new PgPoint(x2, y2));

        PgBox.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(
        new byte[]
        {
            64, 16, 0, 0, 0, 0, 0, 0, 64, 8, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 63, 240, 0,
            0, 0, 0, 0, 0,
        },
        4,
        3,
        2,
        1)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsBox(
        byte[] binaryData,
        double x1,
        double y1,
        double x2,
        double y2)
    {
        var expectedValue = new PgBox(new PgPoint(x1, y1), new PgPoint(x2, y2));
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgBox actualValue = PgBox.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("(4,3),(2,1)", 4, 3, 2, 1)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsBox(
        string textData,
        double x1,
        double y1,
        double x2,
        double y2)
    {
        var expectedValue = new PgBox(new PgPoint(x1, y1), new PgPoint(x2, y2));
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgBox actualValue = PgBox.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("(1,2)")]
    [Arguments("(1,2),(3,4),(5,6)")]
    public async Task DecodeText_Should_Fail_When_LiteralDoesNotHave2Points(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgBox.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgBox");
            await Assert.That(e.Message).Contains("Box geoms must have exactly 2 points");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnBoxType() => await Assert.That(PgBox.DbType).IsEqualTo(PgTypeInfo.Box);

    [Test]
    public async Task ArrayDbType_Should_ReturnBoxType() =>
        await Assert.That(PgBox.ArrayDbType).IsEqualTo(PgTypeInfo.BoxArray);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgBox.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Box, true);
        yield return () => (PgTypeInfo.BoxArray, false);
        yield return () => (PgTypeInfo.Int4, false);
    }
}
