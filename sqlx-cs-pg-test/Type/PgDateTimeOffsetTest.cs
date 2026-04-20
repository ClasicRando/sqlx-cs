using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDateTimeOffset))]
public class PgDateTimeOffsetTest
{
    [Test]
    [Arguments(2024, 1, 1, 0, 1, 45, 36, 92, 0, new byte[] { 0, 223, 112, 212, 218, 87, 185, 60 })]
    [Arguments(1995, 1, 1, 6, 0, 23, 1, 19, 2, new byte[] { 0, 220, 48, 133, 128, 158, 135, 187 })]
    public async Task Encode_Should_WriteDate(
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
        using var buffer = new ArrayBufferWriter();

        PgDateTimeOffset.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 223, 112, 212, 218, 87, 185, 60 }, 2024, 1, 1, 0, 1, 45, 36, 92)]
    [Arguments(new byte[] { 0, 220, 48, 133, 128, 158, 135, 187 }, 1995, 1, 1, 4, 0, 23, 1, 19)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
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
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        DateTimeOffset actualValue = PgDateTimeOffset.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("2024-01-01 00:01:45.036092", 2024, 1, 1, 0, 1, 45, 36, 92)]
    [Arguments("1995-01-01 04:00:23.001019", 1995, 1, 1, 4, 0, 23, 1, 19)]
    [Arguments("1995-01-01 04:00:23.001019+00", 1995, 1, 1, 4, 0, 23, 1, 19)]
    [Arguments("1995-01-01 04:00:23+00", 1995, 1, 1, 4, 0, 23, 0, 0)]
    [Arguments("1995-01-01 04:00:23", 1995, 1, 1, 4, 0, 23, 0, 0)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDate(
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
        var textValue = new PgTextValue(textData, in columnMetadata);

        DateTimeOffset actualValue = PgDateTimeOffset.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_InvalidDatetimeString(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgDateTimeOffset.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.DateTime");
            await Assert.That(e.Message).Contains("Cannot parse");
            await Assert.That(e.Message).Contains("as a DateTime");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnTimestampType() =>
        await Assert.That(PgTypeInfo.Timestamptz).IsEqualTo(PgDateTimeOffset.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnTimestampType() =>
        await Assert.That(PgTypeInfo.TimestamptzArray).IsEqualTo(PgDateTimeOffset.ArrayDbType);

    [Test]
    public async Task RangeType_Should_ReturnTimestampRangeType() =>
        await Assert.That(PgTypeInfo.Tstzrange).IsEqualTo(PgDateTimeOffset.RangeType);

    [Test]
    public async Task RangeArrayType_Should_ReturnTimestampRangeType() =>
        await Assert.That(PgTypeInfo.TstzrangeArray).IsEqualTo(PgDateTimeOffset.RangeArrayType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgDateTimeOffset.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Timestamp, true];
        yield return [PgTypeInfo.Timestamptz, true];
        yield return [PgTypeInfo.TimestampArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
