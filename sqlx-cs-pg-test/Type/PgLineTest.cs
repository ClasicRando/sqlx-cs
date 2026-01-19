using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgLine))]
public class PgLineTest
{
    [Test]
    public async Task Encode_Should_WritePgCircle()
    {
        byte[] expectedBytes =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 16, 0, 0, 0,
            0, 0, 0,
        ];
        var value = new PgLine(5.63, 8.59, 4);
        using var buffer = new PooledArrayBufferWriter();

        PgLine.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsPgCircle()
    {
        var expectedValue = new PgLine(5.63, 8.59, 4);
        byte[] binaryData =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 16, 0, 0, 0,
            0, 0, 0,
        ];
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgLine actualValue = PgLine.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsPgCircle()
    {
        const string textData = "{5.63,8.59,4}";
        var expectedValue = new PgLine(5.63, 8.59, 4);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgLine actualValue = PgLine.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("{error,error,error}", "Could not parse A value")]
    [Arguments("{1,error,error}", "Could not parse B value")]
    [Arguments("{1,2,error}", "Could not parse C value")]
    public async Task DecodeText_Should_Fail_When_InvalidText(
        string textData,
        string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgLine.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgLine");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnLineType() => await Assert.That(PgTypeInfo.Line).IsEqualTo(PgLine.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnLineArrayType() =>
        await Assert.That(PgTypeInfo.LineArray).IsEqualTo(PgLine.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgLine.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Line, true];
        yield return [PgTypeInfo.LineArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
