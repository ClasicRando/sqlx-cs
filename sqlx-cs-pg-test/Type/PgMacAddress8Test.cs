using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgMacAddress8))]
public class PgMacAddress8Test
{
    [Test]
    [Arguments(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public async Task Encode_Should_WriteMacAddr8(byte[] address)
    {
        PgMacAddress8 value = PgMacAddress8.FromBytes(address);
        using var buffer = new PooledArrayBufferWriter();

        PgMacAddress8.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(address);
    }

    [Test]
    [Arguments(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsMacAddr8(byte[] binaryData)
    {
        PgMacAddress8 expectedValue = PgMacAddress8.FromBytes(binaryData);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgMacAddress8 actualValue = PgMacAddress8.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(
        "08:00:2b:01:02:03:04:05",
        new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsMacAddr8(string textData, byte[] address)
    {
        PgMacAddress8 expectedValue = PgMacAddress8.FromBytes(address);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgMacAddress8 actualValue = PgMacAddress8.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("01:01:01:01:01", "Expected 8 address hex characters")]
    [Arguments("01:01:01:01:1:01:01:01", "Could not parse network location bytes from")]
    public async Task DecodeText_Should_Fail_When_InvalidPgMacAddress8(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgMacAddress8 value = PgMacAddress8.DecodeText(textValue);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgMacAddress8");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnMacAddr8Type() =>
        await Assert.That(PgMacAddress8.DbType).IsEqualTo(PgTypeInfo.Macaddr8);

    [Test]
    public async Task ArrayDbType_Should_ReturnMacAddr8Type() =>
        await Assert.That(PgMacAddress8.ArrayDbType).IsEqualTo(PgTypeInfo.Macaddr8Array);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgMacAddress8.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Macaddr8, true];
        yield return [PgTypeInfo.Macaddr8Array, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
