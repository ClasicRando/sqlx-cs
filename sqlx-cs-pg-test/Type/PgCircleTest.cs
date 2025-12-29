using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgCircle))]
public class PgCircleTest
{
    [Test]
    public async Task Encode_Should_WritePgCircle()
    {
        byte[] expectedBytes =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 16, 0, 0, 0,
            0, 0, 0,
        ];
        var value = new PgCircle(new PgPoint(5.63, 8.59), 4);
        using var buffer = new WriteBuffer();

        PgCircle.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsPgCircle()
    {
        var expectedValue = new PgCircle(new PgPoint(5.63, 8.59), 4);
        byte[] binaryData =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 16, 0, 0, 0,
            0, 0, 0,
        ];
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgCircle actualValue = PgCircle.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsPgCircle()
    {
        const string textData = "<(5.63,8.59),4>";
        var expectedValue = new PgCircle(new PgPoint(5.63, 8.59), 4);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgCircle actualValue = PgCircle.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("(error,error),error", "Could not parse X coordinate")]
    [Arguments("<(1,2),error>", "Could not parse radius from ")]
    public async Task DecodeText_Should_Fail_When_InvalidText(
        string textData,
        string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgCircle.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgCircle");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnCircleType() => await Assert.That(PgTypeInfo.Circle).IsEqualTo(PgCircle.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnCircleArrayType() =>
        await Assert.That(PgTypeInfo.CircleArray).IsEqualTo(PgCircle.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgCircle.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Circle, true];
        yield return [PgTypeInfo.CircleArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
