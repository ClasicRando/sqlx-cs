using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDateTime))]
public class PgDateTimeTest
{
    [Test]
    [Arguments(2024, 1, 1, 0, 1, 45, 36, 92, new byte[] { 0, 223, 112, 212, 218, 87, 185, 60 })]
    [Arguments(1995, 1, 1, 4, 0, 23, 1, 19, new byte[] { 0, 220, 48, 133, 128, 158, 135, 187 })]
    public async Task Encode_Should_WriteDateTime(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        byte[] expectedBytes)
    {
        var value = new DateTime(year, month, day, hour, minute, second, millisecond, microsecond);
        using var buffer = new PooledArrayBufferWriter();

        PgDateTime.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 223, 112, 212, 218, 87, 185, 60 }, 2024, 1, 1, 0, 1, 45, 36, 92)]
    [Arguments(new byte[] { 0, 220, 48, 133, 128, 158, 135, 187 }, 1995, 1, 1, 4, 0, 23, 1, 19)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDateTime(
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
        var expectedValue =
            new DateTime(year, month, day, hour, minute, second, millisecond, microsecond);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        DateTime actualValue = PgDateTime.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("2024-01-01 00:01:45.036092", 2024, 1, 1, 0, 1, 45, 36, 92)]
    [Arguments("1995-01-01 04:00:23.001019", 1995, 1, 1, 4, 0, 23, 1, 19)]
    [Arguments("1995-01-01 04:00:23.001019+00", 1995, 1, 1, 4, 0, 23, 1, 19)]
    [Arguments("1995-01-01 04:00:23+00", 1995, 1, 1, 4, 0, 23, 0, 0)]
    [Arguments("1995-01-01 04:00:23", 1995, 1, 1, 4, 0, 23, 0, 0)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDateTime(
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
        var expectedValue =
            new DateTime(year, month, day, hour, minute, second, millisecond, microsecond);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        DateTime actualValue = PgDateTime.DecodeText(textValue);

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
            PgDateTime.DecodeText(textValue);
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
        await Assert.That(PgTypeInfo.Timestamp).IsEqualTo(PgDateTime.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnTimestampType() =>
        await Assert.That(PgTypeInfo.TimestampArray).IsEqualTo(PgDateTime.ArrayDbType);

    [Test]
    public async Task RangeType_Should_ReturnTimestampRangeType() =>
        await Assert.That(PgTypeInfo.Tsrange).IsEqualTo(PgDateTime.RangeType);

    [Test]
    public async Task RangeArrayType_Should_ReturnTimestampRangeType() =>
        await Assert.That(PgTypeInfo.TsrangeArray).IsEqualTo(PgDateTime.RangeArrayType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgDateTime.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Timestamp, true];
        yield return [PgTypeInfo.Timestamptz, true];
        yield return [PgTypeInfo.TimestampArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
