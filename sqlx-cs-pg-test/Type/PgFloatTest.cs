using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgFloat))]
public class PgFloatTest
{
    [Test]
    [Arguments(float.MinValue, new byte[] { 255, 127, 255, 255 })]
    [Arguments(-25.2356, new byte[] { 193, 201, 226, 130 })]
    [Arguments(0, new byte[] { 0, 0, 0, 0 })]
    [Arguments(85.569, new byte[] { 66, 171, 35, 84 })]
    [Arguments(float.MaxValue, new byte[] { 127, 127, 255, 255 })]
    public async Task Encode_Should_WriteFloat(float value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgFloat.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 255, 127, 255, 255 }, float.MinValue)]
    [Arguments(new byte[] { 193, 201, 226, 130 }, -25.2356)]
    [Arguments(new byte[] { 192, 57, 60, 80, 72, 22, 240, 7 }, -25.2356)]
    [Arguments(new byte[] { 0, 0, 0, 0 }, 0)]
    [Arguments(new byte[] { 66, 171, 35, 84 }, 85.569)]
    [Arguments(new byte[] { 64, 85, 100, 106, 126, 249, 219, 35 }, 85.569)]
    [Arguments(new byte[] { 127, 127, 255, 255 }, float.MaxValue)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsFloat(
        byte[] binaryData,
        float expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgFloat.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(new byte[] { 255, 239, 255, 255, 255, 255, 255, 255 })]
    [Arguments(new byte[] { 127, 239, 255, 255, 255, 255, 255, 255 })]
    public async Task DecodeBytes_Should_Fail_When_OutsideOfFloatBounds(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);
        try
        {
            var temp = PgFloat.DecodeBytes(ref binaryValue);
            Assert.Fail($"Decoding should have failed. Found '{temp}'");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Single");
            await Assert.That(e.Message).Contains("Floating point value is outside the bounds of float");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments(new byte[] { 0 })]
    [Arguments(new byte[] { 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public async Task DecodeBytes_Should_Fail_When_InvalidNumberOfBytes(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);
        try
        {
            PgFloat.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Single");
            await Assert.That(e.Message).Contains("Could not extract float from buffer. Number of bytes = ");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments("-3.40282346638528859e+38", float.MinValue)]
    [Arguments("-25.2356", -25.2356)]
    [Arguments("0", 0)]
    [Arguments("85.569", 85.569)]
    [Arguments("3.40282346638528859e+38", float.MaxValue)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsFloat(
        string textData,
        float expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgFloat.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error", "Could not convert 'error' into System.Single")]
    [Arguments("3.40282346638528859e+39", "Floating point value is outside the bounds of float")]
    [Arguments("-3.40282346638528859e+39", "Floating point value is outside the bounds of float")]
    public async Task DecodeText_Should_Fail_When_InvalidFloatString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgFloat.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Single");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnFloatType() => await Assert.That(PgTypeInfo.Float4).IsEqualTo(PgFloat.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnFloatType() =>
        await Assert.That(PgTypeInfo.Float4Array).IsEqualTo(PgFloat.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgFloat.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Float4, true];
        yield return [PgTypeInfo.Float4Array, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
