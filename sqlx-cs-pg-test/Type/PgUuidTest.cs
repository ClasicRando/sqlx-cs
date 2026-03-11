using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBytea))]
public class PgUuidTest
{
    [Test]
    [Arguments(
        "019a22a1-8d4c-7e71-8ac5-e31d330b866c",
        new byte[] { 161, 34, 154, 1, 76, 141, 113, 126, 138, 197, 227, 29, 51, 11, 134, 108 })]
    [Arguments(
        "019a22a1-c5bc-75c5-baf1-8199cfc9d061",
        new byte[] { 161, 34, 154, 1, 188, 197, 197, 117, 186, 241, 129, 153, 207, 201, 208, 97 })]
    public async Task Encode_Should_WriteGuid(string uuid, byte[] address)
    {
        Guid value = Guid.Parse(uuid);
        using var buffer = new PooledArrayBufferWriter();

        PgUuid.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(address);
    }

    [Test]
    [Arguments(
        new byte[] { 153, 34, 154, 1, 142, 184, 211, 115, 146, 27, 178, 250, 187, 200, 138, 60 },
        "019a2299-b88e-73d3-921b-b2fabbc88a3c")]
    [Arguments(
        new byte[] { 153, 34, 154, 1, 251, 251, 61, 121, 174, 159, 194, 153, 226, 118, 209, 34 },
        "019a2299-fbfb-793d-ae9f-c299e276d122")]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsGuid(byte[] binaryData, string uuid)
    {
        Guid expectedValue = Guid.Parse(uuid);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        Guid actualValue = PgUuid.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("019a2299-b88e-73d3-921b-b2fabbc88a3c")]
    [Arguments("019a2299-fbfb-793d-ae9f-c299e276d122")]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsGuid(string textData)
    {
        Guid expectedValue = Guid.Parse(textData);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        Guid actualValue = PgUuid.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_FirstCharacterIsNotValid(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgUuid.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Guid");
            await Assert.That(e.Message).Contains("Could not parse ");
            await Assert.That(e.Message).Contains(" into a Guid");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnUuidType() => await Assert.That(PgTypeInfo.Uuid).IsEqualTo(PgUuid.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnUuidType() =>
        await Assert.That(PgTypeInfo.UuidArray).IsEqualTo(PgUuid.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgUuid.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Uuid, true];
        yield return [PgTypeInfo.UuidArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
