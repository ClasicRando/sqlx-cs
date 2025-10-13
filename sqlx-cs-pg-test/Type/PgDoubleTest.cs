using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDouble))]
public class PgDoubleTest
{
    [Theory]
    [InlineData(double.MinValue, new byte[] { 255, 239, 255, 255, 255, 255, 255, 255 })]
    [InlineData(float.MinValue - 1D, new byte[] { 199, 239, 255, 255, 224, 0, 0, 0 })]
    [InlineData(-25.2356, new byte[] { 192, 57, 60, 80, 72, 22, 240, 7 })]
    [InlineData(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(85.569, new byte[] { 64, 85, 100, 106, 126, 249, 219, 35 })]
    [InlineData(float.MaxValue + 1D, new byte[] { 71, 239, 255, 255, 224, 0, 0, 0 })]
    [InlineData(double.MaxValue, new byte[] { 127, 239, 255, 255, 255, 255, 255, 255 })]
    public void Encode_Should_WriteDouble(double value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgDouble.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 255, 239, 255, 255, 255, 255, 255, 255 }, double.MinValue)]
    [InlineData(new byte[] { 199, 239, 255, 255, 224, 0, 0, 0 }, float.MinValue - 1D)]
    [InlineData(new byte[] { 192, 57, 60, 80, 72, 22, 240, 7 }, -25.2356)]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 64, 85, 100, 106, 126, 249, 219, 35 }, 85.569)]
    [InlineData(new byte[] { 71, 239, 255, 255, 224, 0, 0, 0 }, float.MaxValue + 1D)]
    [InlineData(new byte[] { 127, 239, 255, 255, 255, 255, 255, 255 }, double.MaxValue)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsDouble(
        byte[] binaryData,
        double expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgDouble.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
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
            PgDouble.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Double", e.Message);
            Assert.Contains("Could not extract float from buffer. Number of bytes = ", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData("-1.7976931348623157E+308", double.MinValue)]
    [InlineData("-25.2356", -25.2356)]
    [InlineData("0", 0)]
    [InlineData("85.569", 85.569)]
    [InlineData("1.7976931348623157E+308", double.MaxValue)]
    public void DecodeText_Should_DecodeTextEncodedValueAsDouble(
        string textData,
        double expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgDouble.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_InvalidDoubleString(string textData)
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
            Assert.Contains("Desired Output: System.Double", e.Message);
            Assert.Contains("Could not convert ", e.Message);
            Assert.Contains(" into System.Double", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnFloatType() => Assert.Equal(PgDouble.DbType, PgType.Float8);

    [Fact]
    public void ArrayDbType_Should_ReturnFloatType() =>
        Assert.Equal(PgDouble.ArrayDbType, PgType.Float8Array);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgDouble.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Float8, true];
        yield return [PgType.Float8Array, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Float8, PgDouble.GetActualType(0));
    }
}
