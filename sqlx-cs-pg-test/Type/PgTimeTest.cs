using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgTime))]
public class PgTimeTest
{
    [Test]
    [Arguments(4, 5, 6, 789, 123, new byte[] { 0, 0, 0, 3, 108, 151, 203, 3 })]
    [Arguments(4, 5, 6, 789, 0, new byte[] { 0, 0, 0, 3, 108, 151, 202, 136 })]
    [Arguments(4, 5, 6, 0, 0, new byte[] { 0, 0, 0, 3, 108, 139, 192, 128 })]
    [Arguments(4, 5, 0, 0, 0, new byte[] { 0, 0, 0, 3, 108, 48, 51, 0 })]
    public async Task Encode_Should_WriteDate(
        int hour,
        int minute,
        int second,
        int millisecond,
        int microsecond,
        byte[] expectedBytes)
    {
        var value = new TimeOnly(hour, minute, second, millisecond, microsecond);
        using var buffer = new PooledArrayBufferWriter();

        PgTime.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 151, 203, 3 }, 4, 5, 6, 789, 123)]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 151, 202, 136 }, 4, 5, 6, 789, 0)]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 139, 192, 128 }, 4, 5, 6, 0, 0)]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 48, 51, 0 }, 4, 5, 0, 0, 0)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
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

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("04:05:06.789123", 4, 5, 6, 789, 123)]
    [Arguments("04:05:06.789", 4, 5, 6, 789, 0)]
    [Arguments("04:05:06", 4, 5, 6, 0, 0)]
    [Arguments("04:05:00", 4, 5, 0, 0, 0)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDate(
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

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    [Arguments("28:56:12")]
    public async Task DecodeText_Should_Fail_When_InvalidDateString(string textData)
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
            await Assert.That(e.Message).Contains("Desired Output: System.TimeOnly");
            await Assert.That(e.Message).Contains("Could not parse");
            await Assert.That(e.Message).Contains("into a time value");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnDateType() => await Assert.That(PgTypeInfo.Time).IsEqualTo(PgTime.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnDateType() =>
        await Assert.That(PgTypeInfo.TimeArray).IsEqualTo(PgTime.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgTime.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Time, true];
        yield return [PgTypeInfo.TimeArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
