using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgShort))]
public class PgShortTest
{
    [Test]
    [Arguments(short.MinValue, new byte[] { 128, 0 })]
    [Arguments(0, new byte[] { 0, 0 })]
    [Arguments(short.MaxValue, new byte[] { 127, 255 })]
    public async Task Encode_Should_WriteShort(short value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgShort.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 255, 255, 255, 255, 255, 255, 128, 0 }, short.MinValue)]
    [Arguments(new byte[] { 255, 255, 128, 0 }, short.MinValue)]
    [Arguments(new byte[] { 128, 0 }, short.MinValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [Arguments(new byte[] { 127, 255 }, short.MaxValue)]
    [Arguments(new byte[] { 0, 0, 127, 255 }, short.MaxValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 }, short.MaxValue)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsShort(
        byte[] binaryData,
        short expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        var actualValue = PgShort.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(new byte[] { 128, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 255, 255, 255, 255, 128, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 })]
    [Arguments(new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 })]
    public async Task DecodeBytes_Should_Fail_When_OutsideOfShortBounds(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);
        try
        {
            var temp = PgShort.DecodeBytes(ref binaryValue);
            Assert.Fail($"Decoding should have failed. Found '{temp}'");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Int16");
            await Assert.That(e.Message).Contains("Value is outside of valid short");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments(new byte[] { 0 })]
    [Arguments(new byte[] { 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public async Task DecodeBytes_Should_Fail_When_InvalidNumberOfBytes(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);
        try
        {
            PgShort.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Int16");
            await Assert.That(e.Message).Contains("Could not extract integer from buffer. Number of bytes = ");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments("-32768", short.MinValue)]
    [Arguments("0", 0)]
    [Arguments("32767", short.MaxValue)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsShort(
        string textData,
        short expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        var actualValue = PgShort.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error", "Could not convert 'error' into System.Int16")]
    [Arguments("-9223372036854775808", "Value is outside of valid short")]
    [Arguments("-2147483648", "Value is outside of valid short")]
    [Arguments("2147483647", "Value is outside of valid short")]
    [Arguments("9223372036854775807", "Value is outside of valid short")]
    public async Task DecodeText_Should_Fail_When_InvalidShortString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgShort.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Int16");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnShortType() => await Assert.That(PgTypeInfo.Int2).IsEqualTo(PgShort.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnShortType() =>
        await Assert.That(PgTypeInfo.Int2Array).IsEqualTo(PgShort.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgShort.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Int8, true];
        yield return [PgTypeInfo.Int2Array, false];
        yield return [PgTypeInfo.Int4, true];
        yield return [PgTypeInfo.Int2, true];
    }
}
