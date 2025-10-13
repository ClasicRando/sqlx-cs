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
    [Theory]
    [InlineData(new byte[] { 192, 168, 0, 1 }, 24, new byte[] { 2, 24, 0, 4, 192, 168, 0, 1 })]
    [InlineData(new byte[] { 10, 0, 0, 2 }, 32, new byte[] { 2, 32, 0, 4, 10, 0, 0, 2 })]
    [InlineData(
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        1,
        new byte[] { 3, 1, 0, 16, 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public void Encode_Should_WriteInet(
        byte[] address,
        byte prefixLength,
        byte[] expectedBytes)
    {
        var value = new PgInet(new IPAddress(address), prefixLength);
        using var buffer = new WriteBuffer();

        PgInet.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 2, 24, 0, 4, 192, 168, 0, 1 }, new byte[] { 192, 168, 0, 1 }, 24)]
    [InlineData(new byte[] { 2, 32, 0, 4, 10, 0, 0, 2 }, new byte[] { 10, 0, 0, 2 }, 32)]
    [InlineData(
        new byte[] { 3, 1, 1, 16, 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        1)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsInet(
        byte[] binaryData,
        byte[] address,
        byte prefixLength)
    {
        var expectedValue = new PgInet(new IPAddress(address), prefixLength);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgInet actualValue = PgInet.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("192.168.0.1/24", new byte[] { 192, 168, 0, 1 }, 24)]
    [InlineData("10.0.0.2/32", new byte[] { 10, 0, 0, 2 }, 32)]
    [InlineData(
        "2001:db8:1234::",
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        128)]
    [InlineData(
        "2001:0DB8:AC10:FE01:0000:0000:0000:0000/1",
        new byte[] { 32, 1, 13, 184, 172, 16, 254, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
        1)]
    public void DecodeText_Should_DecodeTextEncodedValueAsInet(
        string textData,
        byte[] address,
        byte prefixLength)
    {
        var expectedValue = new PgInet(new IPAddress(address), prefixLength);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgInet actualValue = PgInet.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(
        "error",
        "Could not parse 'error' into a network value")]
    [InlineData(
        "192.168.0.1/error",
        "Could not parse '192.168.0.1/error' into a network value")]
    public void DecodeText_Should_Fail_When_InvalidPgInet(string textData, string contains)
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
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgInet", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnInetType() =>
        Assert.Equal(PgInet.DbType, PgType.Inet);

    [Fact]
    public void ArrayDbType_Should_ReturnInetType() =>
        Assert.Equal(PgInet.ArrayDbType, PgType.InetArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgInet.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Cidr, true];
        yield return [PgType.Inet, true];
        yield return [PgType.InetArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        var value = new PgInet(new IPAddress([192, 168, 0, 1]), 32);
        Assert.Equal(PgType.Inet, PgInet.GetActualType(value));
    }
}
