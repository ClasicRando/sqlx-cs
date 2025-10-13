using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgTime))]
public class PgTimeTest
{
    [Theory]
    [InlineData(4, 5, 6, 789, 123, new byte[] { 0, 0, 0, 3, 108, 151, 203, 3 })]
    [InlineData(4, 5, 6, 789, 0, new byte[] { 0, 0, 0, 3, 108, 151, 202, 136 })]
    [InlineData(4, 5, 6, 0, 0, new byte[] { 0, 0, 0, 3, 108, 139, 192, 128 })]
    [InlineData(4, 5, 0, 0, 0, new byte[] { 0, 0, 0, 3, 108, 48, 51, 0 })]
    public void Encode_Should_WriteDate(
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        byte[] expectedBytes)
    {
        var value = new TimeOnly(hour, minute, second, millisecond, microsecond);
        using var buffer = new WriteBuffer();

        PgTime.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 151, 203, 3 }, 4, 5, 6, 789, 123)]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 151, 202, 136 }, 4, 5, 6, 789, 0)]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 139, 192, 128 }, 4, 5, 6, 0, 0)]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 48, 51, 0 }, 4, 5, 0, 0, 0)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
        byte[] binaryData,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond)
    {
        var expectedValue = new TimeOnly(hour, minute, second, millisecond, microsecond);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        TimeOnly actualValue = PgTime.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("04:05:06.789123", 4, 5, 6, 789, 123)]
    [InlineData("04:05:06.789", 4, 5, 6, 789, 0)]
    [InlineData("04:05:06", 4, 5, 6, 0, 0)]
    [InlineData("04:05:00", 4, 5, 0, 0, 0)]
    public void DecodeText_Should_DecodeTextEncodedValueAsDate(
        string textData,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond)
    {
        var expectedValue = new TimeOnly(hour, minute, second, millisecond, microsecond);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        TimeOnly actualValue = PgTime.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    [InlineData("28:56:12")]
    public void DecodeText_Should_Fail_When_InvalidDateString(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgTime.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.TimeOnly", e.Message);
            Assert.Contains("Could not parse", e.Message);
            Assert.Contains("into a time value", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnDateType() => Assert.Equal(PgTime.DbType, PgType.Time);

    [Fact]
    public void ArrayDbType_Should_ReturnDateType() =>
        Assert.Equal(PgTime.ArrayDbType, PgType.TimeArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgTime.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Time, true];
        yield return [PgType.TimeArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Time, PgTime.GetActualType(TimeOnly.MaxValue));
    }
}
