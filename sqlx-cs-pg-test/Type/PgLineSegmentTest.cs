using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgLineSegment))]
public class PgLineSegmentTest
{
    [Test]
    public async Task Encode_Should_WritePgCircle()
    {
        byte[] expectedBytes =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225,
            71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        ];
        var value = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        using var buffer = new WriteBuffer();

        PgLineSegment.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsPgCircle()
    {
        var expectedValue = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        byte[] binaryData =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225,
            71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        ];
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgLineSegment actualValue = PgLineSegment.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsPgCircle()
    {
        const string textData = "((5.63,8.59),(4.87,2.8))";
        var expectedValue = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgLineSegment actualValue = PgLineSegment.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("((1,1),(1,1),(1,1))")]
    [Arguments("((1,1))")]
    public async Task DecodeText_Should_Fail_When_InvalidText(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgLineSegment.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgLineSegment");
            await Assert.That(e.Message).Contains("Line segments must have exactly 2 points");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnLineType() => await Assert.That(PgTypeInfo.Lseg).IsEqualTo(PgLineSegment.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnLineArrayType() =>
        await Assert.That(PgTypeInfo.LsegArray).IsEqualTo(PgLineSegment.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgLineSegment.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Lseg, true];
        yield return [PgTypeInfo.LsegArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
