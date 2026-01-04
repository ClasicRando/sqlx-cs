using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgChar))]
public class PgCharTest
{
    [Test]
    [Arguments(sbyte.MinValue, new byte[] { 128 })]
    [Arguments(1, new byte[] { 1 })]
    [Arguments(sbyte.MaxValue, new byte[] { 127 })]
    public async Task Encode_Should_WriteByte(sbyte value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgChar.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 128 }, sbyte.MinValue)]
    [Arguments(new byte[] { 1 }, 1)]
    [Arguments(new byte[] { 127 }, sbyte.MaxValue)]
    [Arguments(new byte[] { }, 0)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsSbyte(
        byte[] binaryData,
        sbyte expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgChar.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("", 0)]
    [Arguments("t", 116)]
    [Arguments("\\147", 103)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsSbyte(
        string textData,
        sbyte expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgChar.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("ex")]
    [Arguments("err")]
    [Arguments("error")]
    [Arguments("error test")]
    public async Task DecodeText_Should_Fail_When_InvalidNumberOfCharacters(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgChar.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.SByte");
            await Assert.That(e.Message).Contains("Received invalid \"char\" text");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnCharType() => await Assert.That(PgTypeInfo.Char).IsEqualTo(PgChar.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnCharType() =>
        await Assert.That(PgTypeInfo.CharArray).IsEqualTo(PgChar.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgChar.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Char, true];
        yield return [PgTypeInfo.CharArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
