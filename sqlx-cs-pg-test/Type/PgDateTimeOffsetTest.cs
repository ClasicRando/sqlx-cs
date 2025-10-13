using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDateTimeOffset))]
public class PgDateTimeOffsetTest
{
    [Theory]
    [InlineData(2024, 1, 1, 0, 1, 45, 36, 92, 0, new byte[] { 0, 223, 112, 212, 218, 87, 185, 60 })]
    [InlineData(1995, 1, 1, 6, 0, 23, 1, 19, 2, new byte[] { 0, 220, 48, 133, 128, 158, 135, 187 })]
    public void Encode_Should_WriteDate(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        int offsetHours,
        byte[] expectedBytes)
    {
        var value = new DateTimeOffset(
            year,
            month,
            day,
            hour,
            minute,
            second,
            millisecond,
            microsecond,
            TimeSpan.FromHours(offsetHours));
        using var buffer = new WriteBuffer();

        PgDateTimeOffset.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0, 223, 112, 212, 218, 87, 185, 60 }, 2024, 1, 1, 0, 1, 45, 36, 92)]
    [InlineData(new byte[] { 0, 220, 48, 133, 128, 158, 135, 187 }, 1995, 1, 1, 4, 0, 23, 1, 19)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
        byte[] binaryData,
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond)
    {
        var expectedValue = new DateTimeOffset(
            year,
            month,
            day,
            hour,
            minute,
            second,
            millisecond,
            microsecond,
            TimeSpan.Zero);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        DateTimeOffset actualValue = PgDateTimeOffset.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("2024-01-01 00:01:45.036092", 2024, 1, 1, 0, 1, 45, 36, 92)]
    [InlineData("1995-01-01 04:00:23.001019", 1995, 1, 1, 4, 0, 23, 1, 19)]
    [InlineData("1995-01-01 04:00:23.001019+00", 1995, 1, 1, 4, 0, 23, 1, 19)]
    [InlineData("1995-01-01 04:00:23+00", 1995, 1, 1, 4, 0, 23, 0, 0)]
    [InlineData("1995-01-01 04:00:23", 1995, 1, 1, 4, 0, 23, 0, 0)]
    public void DecodeText_Should_DecodeTextEncodedValueAsDate(
        string textData,
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond)
    {
        var expectedValue = new DateTimeOffset(
            year,
            month,
            day,
            hour,
            minute,
            second,
            millisecond,
            microsecond,
            TimeSpan.Zero);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        DateTimeOffset actualValue = PgDateTimeOffset.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_InvalidDatetimeString(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgDateTimeOffset.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.DateTime", e.Message);
            Assert.Contains("Cannot parse", e.Message);
            Assert.Contains("as a DateTime", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnTimestampType() =>
        Assert.Equal(PgDateTimeOffset.DbType, PgType.Timestamptz);

    [Fact]
    public void ArrayDbType_Should_ReturnTimestampType() =>
        Assert.Equal(PgDateTimeOffset.ArrayDbType, PgType.TimestamptzArray);

    [Fact]
    public void RangeType_Should_ReturnTimestampRangeType() =>
        Assert.Equal(PgDateTimeOffset.RangeType, PgType.Tstzrange);

    [Fact]
    public void RangeArrayType_Should_ReturnTimestampRangeType() =>
        Assert.Equal(PgDateTimeOffset.RangeArrayType, PgType.TstzrangeArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgDateTimeOffset.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Timestamp, true];
        yield return [PgType.Timestamptz, true];
        yield return [PgType.TimestampArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Timestamptz, PgDateTimeOffset.GetActualType(DateTimeOffset.MaxValue));
    }
}
