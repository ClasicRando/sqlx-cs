using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPoint))]
public class PgPointTest
{
    [Test]
    public async Task Encode_Should_WritePoint()
    {
        byte[] expectedBytes =
            [64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174];
        var value = new PgPoint(5.63, 8.59);
        using var buffer = new ArrayBufferWriter();

        PgPoint.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsPoint()
    {
        byte[] binaryData =
            [64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174];
        var expectedValue = new PgPoint(5.63, 8.59);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgPoint actualValue = PgPoint.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsPoint()
    {
        const string textData = "(5.63,8.59)";
        var expectedValue = new PgPoint(5.63, 8.59);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgPoint actualValue = PgPoint.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("(error)", "Could not find point separator character")]
    [Arguments("(error,1)", "Could not parse X coordinate")]
    [Arguments("(1,error)", "Could not parse Y coordinate")]
    public async Task DecodeText_Should_Fail_When_InvalidText(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgPoint.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgPoint");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnPointType() => await Assert.That(PgTypeInfo.Point).IsEqualTo(PgPoint.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnPointType() =>
        await Assert.That(PgTypeInfo.PointArray).IsEqualTo(PgPoint.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgPoint.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Point, true];
        yield return [PgTypeInfo.PointArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
