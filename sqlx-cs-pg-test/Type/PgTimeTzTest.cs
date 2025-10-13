using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgTimeTz))]
public class PgTimeTzTest
{
    [Theory]
    [InlineData(4, 5, 6, 789, 123, 0, new byte[] { 0, 0, 0, 3, 108, 151, 203, 3, 0, 0, 0, 0 })]
    [InlineData(4, 5, 6, 789, 0, 3600, new byte[] { 0, 0, 0, 3, 108, 151, 202, 136, 0, 0, 14, 16 })]
    [InlineData(
        4,
        5,
        6,
        0,
        0,
        -3600,
        new byte[] { 0, 0, 0, 3, 108, 139, 192, 128, 255, 255, 241, 240 })]
    [InlineData(4, 5, 0, 0, 0, 0, new byte[] { 0, 0, 0, 3, 108, 48, 51, 0, 0, 0, 0, 0 })]
    public void Encode_Should_WriteDate(
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        int offset,
        byte[] expectedBytes)
    {
        var value = new PgTimeTz(
            new TimeOnly(hour, minute, second, millisecond, microsecond),
            offset);
        using var buffer = new WriteBuffer();

        PgTimeTz.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 151, 203, 3, 0, 0, 0, 0 }, 4, 5, 6, 789, 123, 0)]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 151, 202, 136, 0, 0, 14, 16 }, 4, 5, 6, 789, 0, 3600)]
    [InlineData(
        new byte[] { 0, 0, 0, 3, 108, 139, 192, 128, 255, 255, 241, 240 },
        4,
        5,
        6,
        0,
        0,
        -3600)]
    [InlineData(new byte[] { 0, 0, 0, 3, 108, 48, 51, 0, 0, 0, 0, 0 }, 4, 5, 0, 0, 0, 0)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
        byte[] binaryData,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        int offset)
    {
        var expectedValue = new PgTimeTz(
            new TimeOnly(hour, minute, second, millisecond, microsecond),
            offset);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgTimeTz actualValue = PgTimeTz.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("04:05:06.789123Z", 4, 5, 6, 789, 123, 0)]
    [InlineData("04:05:06.789+01", 4, 5, 6, 789, 0, 3600)]
    [InlineData("04:05:06-01", 4, 5, 6, 0, 0, -3600)]
    [InlineData("04:05:00+00", 4, 5, 0, 0, 0, 0)]
    public void DecodeText_Should_DecodeTextEncodedValueAsDate(
        string textData,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        int offset)
    {
        var expectedValue = new PgTimeTz(
            new TimeOnly(hour, minute, second, millisecond, microsecond),
            offset);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgTimeTz actualValue = PgTimeTz.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error", "System.TimeOnly", "Could not parse 'error' into a time value")]
    [InlineData(
        "23:56:12/01",
        "System.TimeOnly",
        "Could not parse '23:56:12/01' into a time value")]
    [InlineData("23:56:12+error", "Sqlx.Postgres.Type.PgTimeTz", "Could not parse offset from")]
    [InlineData("23:56:12+07:52:24", "Sqlx.Postgres.Type.PgTimeTz", "Could not parse offset from")]
    public void DecodeText_Should_Fail_When_InvalidDateString(
        string textData,
        string output,
        string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgTimeTz.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains($"Desired Output: {output}", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnDateType() => Assert.Equal(PgTimeTz.DbType, PgType.Timetz);

    [Fact]
    public void ArrayDbType_Should_ReturnDateType() =>
        Assert.Equal(PgTimeTz.ArrayDbType, PgType.TimetzArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgTimeTz.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Timetz, true];
        yield return [PgType.TimetzArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Timetz, PgTimeTz.GetActualType(new PgTimeTz(TimeOnly.MaxValue, 0)));
    }
}
