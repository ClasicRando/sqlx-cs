using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgLong))]
public class PgLongTest
{
    [Theory]
    [InlineData(long.MinValue, new byte[] { 128, 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(int.MinValue, new byte[] { 255, 255, 255, 255, 128, 0, 0, 0 })]
    [InlineData(short.MinValue, new byte[] { 255, 255, 255, 255, 255, 255, 128, 0 })]
    [InlineData(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(short.MaxValue, new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 })]
    [InlineData(int.MaxValue, new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 })]
    [InlineData(long.MaxValue, new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 })]
    public void Encode_Should_WriteLong(long value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgLong.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 128, 0, 0, 0, 0, 0, 0, 0 }, long.MinValue)]
    [InlineData(new byte[] { 255, 255, 255, 255, 128, 0, 0, 0 }, int.MinValue)]
    [InlineData(new byte[] { 255, 255, 255, 255, 255, 255, 128, 0 }, short.MinValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 }, short.MaxValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 }, int.MaxValue)]
    [InlineData(new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 }, long.MaxValue)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsLong(
        byte[] binaryData,
        long expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgLong.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(new byte[] { 0 })]
    [InlineData(new byte[] { 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public void DecodeBytes_Should_Fail_When_InvalidNumberOfBytes(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);
        try
        {
            PgLong.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Int64", e.Message);
            Assert.Contains("Could not extract integer from buffer. Number of bytes = ", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData("-9223372036854775808", long.MinValue)]
    [InlineData("-2147483648", int.MinValue)]
    [InlineData("-32768", short.MinValue)]
    [InlineData("0", 0)]
    [InlineData("32767", short.MaxValue)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("9223372036854775807", long.MaxValue)]
    public void DecodeText_Should_DecodeTextEncodedValueAsLong(
        string textData,
        long expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgLong.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error", "Could not convert 'error' into System.Int64")]
    public void DecodeText_Should_Fail_When_InvalidLongString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgLong.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Int64", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnLongType() => Assert.Equal(PgLong.DbType, PgTypeInfo.Int8);

    [Fact]
    public void ArrayDbType_Should_ReturnLongType() =>
        Assert.Equal(PgLong.ArrayDbType, PgTypeInfo.Int8Array);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgLong.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Int8, true];
        yield return [PgTypeInfo.Int8Array, false];
        yield return [PgTypeInfo.Int4, true];
        yield return [PgTypeInfo.Int2, true];
    }
}
