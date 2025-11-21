using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDate))]
public class PgDateTest
{
    [Theory]
    [InlineData(2024, 1, 1, new byte[] { 0, 0, 34, 62 })]
    [InlineData(1995, 1, 1, new byte[] { 255, 255, 248, 222 })]
    public void Encode_Should_WriteDate(int year, int month, int day, byte[] expectedBytes)
    {
        var value = new DateOnly(year, month, day);
        using var buffer = new WriteBuffer();

        PgDate.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0, 0, 34, 62 }, 2024, 1, 1)]
    [InlineData(new byte[] { 255, 255, 248, 222 }, 1995, 1, 1)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
        byte[] binaryData,
        int year,
        int month,
        int day)
    {
        var expectedValue = new DateOnly(year, month, day);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        DateOnly actualValue = PgDate.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("2024-01-01", 2024, 1, 1)]
    [InlineData("1995-01-01", 1995, 1, 1)]
    public void DecodeText_Should_DecodeTextEncodedValueAsDate(
        string textData,
        int year,
        int month,
        int day)
    {
        var expectedValue = new DateOnly(year, month, day);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        DateOnly actualValue = PgDate.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("2024/01/01")]
    [InlineData("01/01/1995")]
    public void DecodeText_Should_Fail_When_InvalidDateString(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgDate.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.DateOnly", e.Message);
            Assert.Contains("Cannot parse", e.Message);
            Assert.Contains("as a DateOnly", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnDateType() => Assert.Equal(PgDate.DbType, PgTypeInfo.Date);

    [Fact]
    public void ArrayDbType_Should_ReturnDateType() =>
        Assert.Equal(PgDate.ArrayDbType, PgTypeInfo.DateArray);

    [Fact]
    public void RangeType_Should_ReturnDateRangeType() =>
        Assert.Equal(PgDate.RangeType, PgTypeInfo.Daterange);

    [Fact]
    public void RangeArrayType_Should_ReturnDateRangeType() =>
        Assert.Equal(PgDate.RangeArrayType, PgTypeInfo.DaterangeArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgDate.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Date, true];
        yield return [PgTypeInfo.DateArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
