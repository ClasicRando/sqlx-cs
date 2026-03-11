using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgTimeTz))]
public class PgTimeTzTest
{
    [Test]
    [Arguments(4, 5, 6, 789, 123, 0, new byte[] { 0, 0, 0, 3, 108, 151, 203, 3, 0, 0, 0, 0 })]
    [Arguments(4, 5, 6, 789, 0, 3600, new byte[] { 0, 0, 0, 3, 108, 151, 202, 136, 0, 0, 14, 16 })]
    [Arguments(
        4,
        5,
        6,
        0,
        0,
        -3600,
        new byte[] { 0, 0, 0, 3, 108, 139, 192, 128, 255, 255, 241, 240 })]
    [Arguments(4, 5, 0, 0, 0, 0, new byte[] { 0, 0, 0, 3, 108, 48, 51, 0, 0, 0, 0, 0 })]
    public async Task Encode_Should_WriteDate(
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
        using var buffer = new PooledArrayBufferWriter();

        PgTimeTz.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 151, 203, 3, 0, 0, 0, 0 }, 4, 5, 6, 789, 123, 0)]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 151, 202, 136, 0, 0, 14, 16 }, 4, 5, 6, 789, 0, 3600)]
    [Arguments(
        new byte[] { 0, 0, 0, 3, 108, 139, 192, 128, 255, 255, 241, 240 },
        4,
        5,
        6,
        0,
        0,
        -3600)]
    [Arguments(new byte[] { 0, 0, 0, 3, 108, 48, 51, 0, 0, 0, 0, 0 }, 4, 5, 0, 0, 0, 0)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
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
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgTimeTz actualValue = PgTimeTz.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("04:05:06.789123Z", 4, 5, 6, 789, 123, 0)]
    [Arguments("04:05:06.789+01", 4, 5, 6, 789, 0, 3600)]
    [Arguments("04:05:06-01", 4, 5, 6, 0, 0, -3600)]
    [Arguments("04:05:00+00", 4, 5, 0, 0, 0, 0)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDate(
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
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgTimeTz actualValue = PgTimeTz.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error", "System.TimeOnly", "Could not parse 'error' into a time value")]
    [Arguments(
        "23:56:12/01",
        "System.TimeOnly",
        "Could not parse '23:56:12/01' into a time value")]
    [Arguments("23:56:12+error", "Sqlx.Postgres.Type.PgTimeTz", "Could not parse offset from")]
    [Arguments("23:56:12+07:52:24", "Sqlx.Postgres.Type.PgTimeTz", "Could not parse offset from")]
    public async Task DecodeText_Should_Fail_When_InvalidDateString(
        string textData,
        string output,
        string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgTimeTz.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains($"Desired Output: {output}");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnDateType() => await Assert.That(PgTypeInfo.Timetz).IsEqualTo(PgTimeTz.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnDateType() =>
        await Assert.That(PgTypeInfo.TimetzArray).IsEqualTo(PgTimeTz.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgTimeTz.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Timetz, true];
        yield return [PgTypeInfo.TimetzArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
