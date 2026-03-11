using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgMacAddress))]
public class PgMacAddressTest
{
    [Test]
    [Arguments(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03 })]
    public async Task Encode_Should_WriteMacAddr(byte[] address)
    {
        PgMacAddress value = PgMacAddress.FromBytes(address);
        using var buffer = new PooledArrayBufferWriter();

        PgMacAddress.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(address);
    }

    [Test]
    [Arguments(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03 })]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsMacAddr(byte[] binaryData)
    {
        PgMacAddress expectedValue = PgMacAddress.FromBytes(binaryData);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgMacAddress actualValue = PgMacAddress.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("08:00:2b:01:02:03", new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03 })]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsMacAddr(string textData, byte[] address)
    {
        PgMacAddress expectedValue = PgMacAddress.FromBytes(address);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgMacAddress actualValue = PgMacAddress.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("01:01:01:01:01", "Expected 6 address hex characters")]
    [Arguments("01:01:01:01:1:01", "Could not parse network location bytes from")]
    public async Task DecodeText_Should_Fail_When_InvalidPgMacAddress(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgMacAddress value = PgMacAddress.DecodeText(textValue);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgMacAddress");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnMacAddrType() =>
        await Assert.That(PgTypeInfo.Macaddr).IsEqualTo(PgMacAddress.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnMacAddrType() =>
        await Assert.That(PgTypeInfo.MacaddrArray).IsEqualTo(PgMacAddress.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgMacAddress.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Macaddr, true];
        yield return [PgTypeInfo.MacaddrArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
