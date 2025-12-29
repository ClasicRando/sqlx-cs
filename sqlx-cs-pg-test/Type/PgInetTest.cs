using System.Net;
using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgInet))]
public class PgInetTest
{
    [Test]
    [Arguments(new byte[] { 192, 168, 0, 1 }, 24, new byte[] { 2, 24, 0, 4, 192, 168, 0, 1 })]
    [Arguments(new byte[] { 10, 0, 0, 2 }, 32, new byte[] { 2, 32, 0, 4, 10, 0, 0, 2 })]
    [Arguments(
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        1,
        new byte[] { 3, 1, 0, 16, 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public async Task Encode_Should_WriteInet(
        byte[] address,
        byte prefixLength,
        byte[] expectedBytes)
    {
        var value = new PgInet(new IPAddress(address), prefixLength);
        using var buffer = new WriteBuffer();

        PgInet.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 2, 24, 0, 4, 192, 168, 0, 1 }, new byte[] { 192, 168, 0, 1 }, 24)]
    [Arguments(new byte[] { 2, 32, 0, 4, 10, 0, 0, 2 }, new byte[] { 10, 0, 0, 2 }, 32)]
    [Arguments(
        new byte[] { 3, 1, 1, 16, 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        1)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsInet(
        byte[] binaryData,
        byte[] address,
        byte prefixLength)
    {
        var expectedValue = new PgInet(new IPAddress(address), prefixLength);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgInet actualValue = PgInet.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("192.168.0.1/24", new byte[] { 192, 168, 0, 1 }, 24)]
    [Arguments("10.0.0.2/32", new byte[] { 10, 0, 0, 2 }, 32)]
    [Arguments(
        "2001:db8:1234::",
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        128)]
    [Arguments(
        "2001:0DB8:AC10:FE01:0000:0000:0000:0000/1",
        new byte[] { 32, 1, 13, 184, 172, 16, 254, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
        1)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsInet(
        string textData,
        byte[] address,
        byte prefixLength)
    {
        var expectedValue = new PgInet(new IPAddress(address), prefixLength);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgInet actualValue = PgInet.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(
        "error",
        "Could not parse 'error' into a network value")]
    [Arguments(
        "192.168.0.1/error",
        "Could not parse '192.168.0.1/error' into a network value")]
    public async Task DecodeText_Should_Fail_When_InvalidPgInet(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgInet value = PgInet.DecodeText(textValue);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgInet");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnInetType() =>
        await Assert.That(PgTypeInfo.Inet).IsEqualTo(PgInet.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnInetType() =>
        await Assert.That(PgTypeInfo.InetArray).IsEqualTo(PgInet.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgInet.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Cidr, true];
        yield return [PgTypeInfo.Inet, true];
        yield return [PgTypeInfo.InetArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
