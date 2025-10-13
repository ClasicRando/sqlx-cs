using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDecimal))]
public class PgDecimalTest
{
    [Theory]
    [InlineData("0", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData("1", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 })]
    [InlineData("10", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 10 })]
    [InlineData("100", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 100 })]
    [InlineData("10000", new byte[] { 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 })]
    [InlineData("12345", new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 0, 1, 9, 41 })]
    [InlineData("0.1", new byte[] { 0, 1, 255, 255, 0, 0, 0, 1, 3, 232 })]
    [InlineData("0.01", new byte[] { 0, 1, 255, 255, 0, 0, 0, 2, 0, 100 })]
    [InlineData("0.012", new byte[] { 0, 1, 255, 255, 0, 0, 0, 3, 0, 120 })]
    [InlineData("1.2345", new byte[] { 0, 2, 0, 0, 0, 0, 0, 4, 0, 1, 9, 41 })]
    [InlineData("0.12345", new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 4, 210, 19, 136 })]
    [InlineData("0.01234", new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 0, 123, 15, 160 })]
    [InlineData("12345.67890", new byte[] { 0, 4, 0, 1, 0, 0, 0, 5, 0, 1, 9, 41, 26, 133, 0, 0 })]
    [InlineData("0.00001234", new byte[] { 0, 1, 255, 254, 0, 0, 0, 8, 4, 210 })]
    [InlineData("1234", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 4, 210 })]
    [InlineData("-1234", new byte[] { 0, 1, 0, 0, 64, 0, 0, 0, 4, 210 })]
    [InlineData("12345678", new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 4, 210, 22, 46 })]
    [InlineData("-12345678", new byte[] { 0, 2, 0, 1, 64, 0, 0, 0, 4, 210, 22, 46 })]
    public void Encode_Should_WriteDecimal(string str, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();
        var value = decimal.Parse(str);

        PgDecimal.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 }, 1)]
    [InlineData(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 10 }, 10)]
    [InlineData(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 100 }, 100)]
    [InlineData(new byte[] { 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 }, 10_000)]
    [InlineData(new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 0, 1, 9, 41 }, 12_345)]
    [InlineData(new byte[] { 0, 1, 255, 255, 0, 0, 0, 1, 3, 232 }, 0.1)]
    [InlineData(new byte[] { 0, 1, 255, 255, 0, 0, 0, 2, 0, 100 }, 0.01)]
    [InlineData(new byte[] { 0, 1, 255, 255, 0, 0, 0, 3, 0, 120 }, 0.012)]
    [InlineData(new byte[] { 0, 2, 0, 0, 0, 0, 0, 4, 0, 1, 9, 41 }, 1.2345)]
    [InlineData(new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 4, 210, 19, 136 }, 0.12345)]
    [InlineData(new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 0, 123, 15, 160 }, 0.01234)]
    [InlineData(new byte[] { 0, 3, 0, 1, 0, 0, 0, 5, 0, 1, 9, 41, 26, 133 }, 12345.67890)]
    [InlineData(new byte[] { 0, 1, 255, 254, 0, 0, 0, 8, 4, 210 }, 0.00001234)]
    [InlineData(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 4, 210 }, 1234)]
    [InlineData(new byte[] { 0, 1, 0, 0, 64, 0, 0, 0, 4, 210 }, -1234)]
    [InlineData(new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 4, 210, 22, 46 }, 12345678)]
    [InlineData(new byte[] { 0, 2, 0, 1, 64, 0, 0, 0, 4, 210, 22, 46 }, -12345678)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsDecimal(
        byte[] binaryData,
        decimal expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgDecimal.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("10", 10)]
    [InlineData("100", 100)]
    [InlineData("10000", 10_000)]
    [InlineData("12345", 12_345)]
    [InlineData("0.1", 0.1)]
    [InlineData("0.01", 0.01)]
    [InlineData("0.012", 0.012)]
    [InlineData("1.2345", 1.2345)]
    [InlineData("0.12345", 0.12345)]
    [InlineData("0.01234", 0.01234)]
    [InlineData("12345.67890", 12345.67890)]
    [InlineData("0.00001234", 0.00001234)]
    [InlineData("1234", 1234)]
    [InlineData("-1234", -1234)]
    [InlineData("12345678", 12345678)]
    [InlineData("-12345678", -12345678)]
    public void DecodeText_Should_DecodeTextEncodedValueAsDecimal(
        string textData,
        decimal expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgDecimal.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_InvalidDecimalString(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgDecimal.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Decimal", e.Message);
            Assert.Contains("Cannot convert", e.Message);
            Assert.Contains("to a decimal value", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnNumericType() => Assert.Equal(PgDecimal.DbType, PgType.Numeric);

    [Fact]
    public void ArrayDbType_Should_ReturnNumericType() =>
        Assert.Equal(PgDecimal.ArrayDbType, PgType.NumericArray);

    [Fact]
    public void RangeType_Should_ReturnNumericRangeType() =>
        Assert.Equal(PgDecimal.RangeType, PgType.Numrange);

    [Fact]
    public void RangeArrayType_Should_ReturnNumericRangeType() =>
        Assert.Equal(PgDecimal.RangeArrayType, PgType.NumrangeArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgDecimal.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Numeric, true];
        yield return [PgType.NumericArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Numeric, PgDecimal.GetActualType(decimal.Zero));
    }
}
