using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgDecimal))]
public class PgDecimalTest
{
    [Test]
    [Arguments("0", new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments("1", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 })]
    [Arguments("10", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 10 })]
    [Arguments("100", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 100 })]
    [Arguments("10000", new byte[] { 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 })]
    [Arguments("12345", new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 0, 1, 9, 41 })]
    [Arguments("0.1", new byte[] { 0, 1, 255, 255, 0, 0, 0, 1, 3, 232 })]
    [Arguments("0.01", new byte[] { 0, 1, 255, 255, 0, 0, 0, 2, 0, 100 })]
    [Arguments("0.012", new byte[] { 0, 1, 255, 255, 0, 0, 0, 3, 0, 120 })]
    [Arguments("1.2345", new byte[] { 0, 2, 0, 0, 0, 0, 0, 4, 0, 1, 9, 41 })]
    [Arguments("0.12345", new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 4, 210, 19, 136 })]
    [Arguments("0.01234", new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 0, 123, 15, 160 })]
    [Arguments("12345.67890", new byte[] { 0, 4, 0, 1, 0, 0, 0, 5, 0, 1, 9, 41, 26, 133, 0, 0 })]
    [Arguments("0.00001234", new byte[] { 0, 1, 255, 254, 0, 0, 0, 8, 4, 210 })]
    [Arguments("1234", new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 4, 210 })]
    [Arguments("-1234", new byte[] { 0, 1, 0, 0, 64, 0, 0, 0, 4, 210 })]
    [Arguments("12345678", new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 4, 210, 22, 46 })]
    [Arguments("-12345678", new byte[] { 0, 2, 0, 1, 64, 0, 0, 0, 4, 210, 22, 46 })]
    public async Task Encode_Should_WriteDecimal(string str, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();
        var value = decimal.Parse(str);

        PgDecimal.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [Arguments(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 }, 1)]
    [Arguments(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 10 }, 10)]
    [Arguments(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 100 }, 100)]
    [Arguments(new byte[] { 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 }, 10_000)]
    [Arguments(new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 0, 1, 9, 41 }, 12_345)]
    [Arguments(new byte[] { 0, 1, 255, 255, 0, 0, 0, 1, 3, 232 }, 0.1)]
    [Arguments(new byte[] { 0, 1, 255, 255, 0, 0, 0, 2, 0, 100 }, 0.01)]
    [Arguments(new byte[] { 0, 1, 255, 255, 0, 0, 0, 3, 0, 120 }, 0.012)]
    [Arguments(new byte[] { 0, 2, 0, 0, 0, 0, 0, 4, 0, 1, 9, 41 }, 1.2345)]
    [Arguments(new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 4, 210, 19, 136 }, 0.12345)]
    [Arguments(new byte[] { 0, 2, 255, 255, 0, 0, 0, 5, 0, 123, 15, 160 }, 0.01234)]
    [Arguments(new byte[] { 0, 3, 0, 1, 0, 0, 0, 5, 0, 1, 9, 41, 26, 133 }, 12345.67890)]
    [Arguments(new byte[] { 0, 1, 255, 254, 0, 0, 0, 8, 4, 210 }, 0.00001234)]
    [Arguments(new byte[] { 0, 1, 0, 0, 0, 0, 0, 0, 4, 210 }, 1234)]
    [Arguments(new byte[] { 0, 1, 0, 0, 64, 0, 0, 0, 4, 210 }, -1234)]
    [Arguments(new byte[] { 0, 2, 0, 1, 0, 0, 0, 0, 4, 210, 22, 46 }, 12345678)]
    [Arguments(new byte[] { 0, 2, 0, 1, 64, 0, 0, 0, 4, 210, 22, 46 }, -12345678)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsDecimal(
        byte[] binaryData,
        decimal expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgDecimal.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("0", 0)]
    [Arguments("1", 1)]
    [Arguments("10", 10)]
    [Arguments("100", 100)]
    [Arguments("10000", 10_000)]
    [Arguments("12345", 12_345)]
    [Arguments("0.1", 0.1)]
    [Arguments("0.01", 0.01)]
    [Arguments("0.012", 0.012)]
    [Arguments("1.2345", 1.2345)]
    [Arguments("0.12345", 0.12345)]
    [Arguments("0.01234", 0.01234)]
    [Arguments("12345.67890", 12345.67890)]
    [Arguments("0.00001234", 0.00001234)]
    [Arguments("1234", 1234)]
    [Arguments("-1234", -1234)]
    [Arguments("12345678", 12345678)]
    [Arguments("-12345678", -12345678)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsDecimal(
        string textData,
        decimal expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgDecimal.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_InvalidDecimalString(string textData)
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
            await Assert.That(e.Message).Contains("Desired Output: System.Decimal");
            await Assert.That(e.Message).Contains("Cannot convert");
            await Assert.That(e.Message).Contains("to a decimal value");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnNumericType() => await Assert.That(PgTypeInfo.Numeric).IsEqualTo(PgDecimal.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnNumericType() =>
        await Assert.That(PgTypeInfo.NumericArray).IsEqualTo(PgDecimal.ArrayDbType);

    [Test]
    public async Task RangeType_Should_ReturnNumericRangeType() =>
        await Assert.That(PgTypeInfo.Numrange).IsEqualTo(PgDecimal.RangeType);

    [Test]
    public async Task RangeArrayType_Should_ReturnNumericRangeType() =>
        await Assert.That(PgTypeInfo.NumrangeArray).IsEqualTo(PgDecimal.RangeArrayType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgDecimal.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Numeric, true];
        yield return [PgTypeInfo.NumericArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
