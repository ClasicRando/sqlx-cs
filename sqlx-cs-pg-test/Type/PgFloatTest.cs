using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgFloat))]
public class PgFloatTest
{
    [Theory]
    [InlineData(float.MinValue, new byte[] { 255, 127, 255, 255 })]
    [InlineData(-25.2356, new byte[] { 193, 201, 226, 130 })]
    [InlineData(0, new byte[] { 0, 0, 0, 0 })]
    [InlineData(85.569, new byte[] { 66, 171, 35, 84 })]
    [InlineData(float.MaxValue, new byte[] { 127, 127, 255, 255 })]
    public void Encode_Should_WriteFloat(float value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgFloat.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 255, 127, 255, 255 }, float.MinValue)]
    [InlineData(new byte[] { 193, 201, 226, 130 }, -25.2356)]
    [InlineData(new byte[] { 192, 57, 60, 80, 72, 22, 240, 7 }, -25.2356)]
    [InlineData(new byte[] { 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 66, 171, 35, 84 }, 85.569)]
    [InlineData(new byte[] { 64, 85, 100, 106, 126, 249, 219, 35 }, 85.569)]
    [InlineData(new byte[] { 127, 127, 255, 255 }, float.MaxValue)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsFloat(
        byte[] binaryData,
        float expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgFloat.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(new byte[] { 255, 239, 255, 255, 255, 255, 255, 255 })]
    [InlineData(new byte[] { 127, 239, 255, 255, 255, 255, 255, 255 })]
    public void DecodeBytes_Should_Fail_When_OutsideOfFloatBounds(byte[] binaryData)
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
            Assert.Contains("Desired Output: System.Single", e.Message);
            Assert.Contains("Floating point value is outside the bounds of float", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData(new byte[] { 0 })]
    [InlineData(new byte[] { 0, 0 })]
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
            PgFloat.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Single", e.Message);
            Assert.Contains("Could not extract float from buffer. Number of bytes = ", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData("-3.40282346638528859e+38", float.MinValue)]
    [InlineData("-25.2356", -25.2356)]
    [InlineData("0", 0)]
    [InlineData("85.569", 85.569)]
    [InlineData("3.40282346638528859e+38", float.MaxValue)]
    public void DecodeText_Should_DecodeTextEncodedValueAsFloat(
        string textData,
        float expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgFloat.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error", "Could not convert 'error' into System.Single")]
    [InlineData("3.40282346638528859e+39", "Floating point value is outside the bounds of float")]
    [InlineData("-3.40282346638528859e+39", "Floating point value is outside the bounds of float")]
    public void DecodeText_Should_Fail_When_InvalidFloatString(string textData, string contains)
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
            Assert.Contains("Desired Output: System.Single", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnFloatType() => Assert.Equal(PgFloat.DbType, PgTypeInfo.Float4);

    [Fact]
    public void ArrayDbType_Should_ReturnFloatType() =>
        Assert.Equal(PgFloat.ArrayDbType, PgTypeInfo.Float4Array);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgFloat.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Float4, true];
        yield return [PgTypeInfo.Float4Array, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
