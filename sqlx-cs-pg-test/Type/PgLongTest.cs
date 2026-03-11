using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgLong))]
public class PgLongTest
{
    [Test]
    [Arguments(long.MinValue, new byte[] { 128, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(int.MinValue, new byte[] { 255, 255, 255, 255, 128, 0, 0, 0 })]
    [Arguments(short.MinValue, new byte[] { 255, 255, 255, 255, 255, 255, 128, 0 })]
    [Arguments(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(short.MaxValue, new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 })]
    [Arguments(int.MaxValue, new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 })]
    [Arguments(long.MaxValue, new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 })]
    public async Task Encode_Should_WriteLong(long value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgLong.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 128, 0, 0, 0, 0, 0, 0, 0 }, long.MinValue)]
    [Arguments(new byte[] { 255, 255, 255, 255, 128, 0, 0, 0 }, int.MinValue)]
    [Arguments(new byte[] { 255, 255, 255, 255, 255, 255, 128, 0 }, short.MinValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 }, short.MaxValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 }, int.MaxValue)]
    [Arguments(new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 }, long.MaxValue)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsLong(
        byte[] binaryData,
        long expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        var actualValue = PgLong.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
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
            PgLong.DecodeBytes(binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Int64");
            await Assert.That(e.Message).Contains("Could not extract integer from buffer. Number of bytes = ");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments("-9223372036854775808", long.MinValue)]
    [Arguments("-2147483648", int.MinValue)]
    [Arguments("-32768", short.MinValue)]
    [Arguments("0", 0)]
    [Arguments("32767", short.MaxValue)]
    [Arguments("2147483647", int.MaxValue)]
    [Arguments("9223372036854775807", long.MaxValue)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsLong(
        string textData,
        long expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        var actualValue = PgLong.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error", "Could not convert 'error' into System.Int64")]
    public async Task DecodeText_Should_Fail_When_InvalidLongString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgLong.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Int64");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnLongType() => await Assert.That(PgTypeInfo.Int8).IsEqualTo(PgLong.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnLongType() =>
        await Assert.That(PgTypeInfo.Int8Array).IsEqualTo(PgLong.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgLong.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Int8, true];
        yield return [PgTypeInfo.Int8Array, false];
        yield return [PgTypeInfo.Int4, true];
        yield return [PgTypeInfo.Int2, true];
    }
}
