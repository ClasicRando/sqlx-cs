using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDate))]
public class PgDateTest
{
    [Test]
    [Arguments(2024, 1, 1, new byte[] { 0, 0, 34, 62 })]
    [Arguments(1995, 1, 1, new byte[] { 255, 255, 248, 222 })]
    public async Task Encode_Should_WriteDate(int year, int month, int day, byte[] expectedBytes)
    {
        var value = new DateOnly(year, month, day);
        using var buffer = new PooledArrayBufferWriter();

        PgDate.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 0, 34, 62 }, 2024, 1, 1)]
    [Arguments(new byte[] { 255, 255, 248, 222 }, 1995, 1, 1)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDate(
        byte[] binaryData,
        int year,
        int month,
        int day)
    {
        var expectedValue = new DateOnly(year, month, day);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        DateOnly actualValue = PgDate.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("2024-01-01", 2024, 1, 1)]
    [Arguments("1995-01-01", 1995, 1, 1)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDate(
        string textData,
        int year,
        int month,
        int day)
    {
        var expectedValue = new DateOnly(year, month, day);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        DateOnly actualValue = PgDate.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("2024/01/01")]
    [Arguments("01/01/1995")]
    public async Task DecodeText_Should_Fail_When_InvalidDateString(string textData)
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
            await Assert.That(e.Message).Contains("Desired Output: System.DateOnly");
            await Assert.That(e.Message).Contains("Cannot parse");
            await Assert.That(e.Message).Contains("as a DateOnly");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnDateType() => await Assert.That(PgTypeInfo.Date).IsEqualTo(PgDate.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnDateType() =>
        await Assert.That(PgTypeInfo.DateArray).IsEqualTo(PgDate.ArrayDbType);

    [Test]
    public async Task RangeType_Should_ReturnDateRangeType() =>
        await Assert.That(PgTypeInfo.Daterange).IsEqualTo(PgDate.RangeType);

    [Test]
    public async Task RangeArrayType_Should_ReturnDateRangeType() =>
        await Assert.That(PgTypeInfo.DaterangeArray).IsEqualTo(PgDate.RangeArrayType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgDate.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Date, true];
        yield return [PgTypeInfo.DateArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
