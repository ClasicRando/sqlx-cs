using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgShort))]
public class PgShortTest
{
    [Theory]
    [InlineData(short.MinValue, new byte[] { 128, 0 })]
    [InlineData(0, new byte[] { 0, 0 })]
    [InlineData(short.MaxValue, new byte[] { 127, 255 })]
    public void Encode_Should_WriteShort(short value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgShort.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 255, 255, 255, 255, 255, 255, 128, 0 }, short.MinValue)]
    [InlineData(new byte[] { 255, 255, 128, 0 }, short.MinValue)]
    [InlineData(new byte[] { 128, 0 }, short.MinValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 127, 255 }, short.MaxValue)]
    [InlineData(new byte[] { 0, 0, 127, 255 }, short.MaxValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 }, short.MaxValue)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsShort(
        byte[] binaryData,
        short expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgShort.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(new byte[] { 128, 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 255, 255, 255, 255, 128, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 })]
    [InlineData(new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 })]
    public void DecodeBytes_Should_Fail_When_OutsideOfShortBounds(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);
        try
        {
            var temp = PgShort.DecodeBytes(ref binaryValue);
            Assert.Fail($"Decoding should have failed. Found '{temp}'");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Int16", e.Message);
            Assert.Contains("Value is outside of valid short", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
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
            PgShort.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Int16", e.Message);
            Assert.Contains("Could not extract integer from buffer. Number of bytes = ", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData("-32768", short.MinValue)]
    [InlineData("0", 0)]
    [InlineData("32767", short.MaxValue)]
    public void DecodeText_Should_DecodeTextEncodedValueAsShort(
        string textData,
        short expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgShort.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error", "Could not convert 'error' into System.Int16")]
    [InlineData("-9223372036854775808", "Value is outside of valid short")]
    [InlineData("-2147483648", "Value is outside of valid short")]
    [InlineData("2147483647", "Value is outside of valid short")]
    [InlineData("9223372036854775807", "Value is outside of valid short")]
    public void DecodeText_Should_Fail_When_InvalidShortString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgShort.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Int16", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnShortType() => Assert.Equal(PgShort.DbType, PgType.Int2);

    [Fact]
    public void ArrayDbType_Should_ReturnShortType() =>
        Assert.Equal(PgShort.ArrayDbType, PgType.Int2Array);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgShort.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Int8, true];
        yield return [PgType.Int2Array, false];
        yield return [PgType.Int4, true];
        yield return [PgType.Int2, true];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Int2, PgShort.GetActualType(0));
    }
}
