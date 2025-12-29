using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDouble))]
public class PgDoubleTest
{
    [Test]
    [Arguments(double.MinValue, new byte[] { 255, 239, 255, 255, 255, 255, 255, 255 })]
    [Arguments(float.MinValue - 1D, new byte[] { 199, 239, 255, 255, 224, 0, 0, 0 })]
    [Arguments(-25.2356, new byte[] { 192, 57, 60, 80, 72, 22, 240, 7 })]
    [Arguments(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(85.569, new byte[] { 64, 85, 100, 106, 126, 249, 219, 35 })]
    [Arguments(float.MaxValue + 1D, new byte[] { 71, 239, 255, 255, 224, 0, 0, 0 })]
    [Arguments(double.MaxValue, new byte[] { 127, 239, 255, 255, 255, 255, 255, 255 })]
    public async Task Encode_Should_WriteDouble(double value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgDouble.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 255, 239, 255, 255, 255, 255, 255, 255 }, double.MinValue)]
    [Arguments(new byte[] { 199, 239, 255, 255, 224, 0, 0, 0 }, float.MinValue - 1D)]
    [Arguments(new byte[] { 192, 57, 60, 80, 72, 22, 240, 7 }, -25.2356)]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [Arguments(new byte[] { 64, 85, 100, 106, 126, 249, 219, 35 }, 85.569)]
    [Arguments(new byte[] { 71, 239, 255, 255, 224, 0, 0, 0 }, float.MaxValue + 1D)]
    [Arguments(new byte[] { 127, 239, 255, 255, 255, 255, 255, 255 }, double.MaxValue)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDouble(
        byte[] binaryData,
        double expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgDouble.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
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
            PgDouble.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Double");
            await Assert.That(e.Message).Contains("Could not extract float from buffer. Number of bytes = ");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments("-1.7976931348623157E+308", double.MinValue)]
    [Arguments("-25.2356", -25.2356)]
    [Arguments("0", 0)]
    [Arguments("85.569", 85.569)]
    [Arguments("1.7976931348623157E+308", double.MaxValue)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDouble(
        string textData,
        double expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgDouble.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_InvalidDoubleString(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgDouble.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Double");
            await Assert.That(e.Message).Contains("Could not convert ");
            await Assert.That(e.Message).Contains(" into System.Double");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnFloatType() => await Assert.That(PgTypeInfo.Float8).IsEqualTo(PgDouble.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnFloatType() =>
        await Assert.That(PgTypeInfo.Float8Array).IsEqualTo(PgDouble.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgDouble.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Float8, true];
        yield return [PgTypeInfo.Float8Array, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
