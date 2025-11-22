using System.Net;
using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgIpNetwork))]
public class PgIpNetworkTest
{
    [Theory]
    [InlineData(new byte[] { 192, 168, 0, 0 }, 24, new byte[] { 2, 24, 1, 4, 192, 168, 0, 0 })]
    [InlineData(new byte[] { 10, 0, 0, 0 }, 32, new byte[] { 2, 32, 1, 4, 10, 0, 0, 0 })]
    [InlineData(
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        48,
        new byte[] { 3, 48, 1, 16, 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public void Encode_Should_WriteIPNetwork(
        byte[] address,
        byte prefixLength,
        byte[] expectedBytes)
    {
        var value = new IPNetwork(new IPAddress(address), prefixLength);
        using var buffer = new WriteBuffer();

        PgIpNetwork.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 2, 24, 1, 4, 192, 168, 0, 0 }, new byte[] { 192, 168, 0, 0 }, 24)]
    [InlineData(new byte[] { 2, 32, 1, 4, 10, 0, 0, 0 }, new byte[] { 10, 0, 0, 0 }, 32)]
    [InlineData(
        new byte[] { 3, 48, 0, 16, 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        48)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsIPNetwork(
        byte[] binaryData,
        byte[] address,
        byte prefixLength)
    {
        var expectedValue = new IPNetwork(new IPAddress(address), prefixLength);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        IPNetwork actualValue = PgIpNetwork.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("192.168.0.0/24", new byte[] { 192, 168, 0, 0 }, 24)]
    [InlineData("10.0.0.0/32", new byte[] { 10, 0, 0, 0 }, 32)]
    [InlineData(
        "2001:db8:1234::/48",
        new byte[] { 32, 1, 13, 184, 18, 52, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        48)]
    [InlineData(
        "2001:0DB8:AC10:FE01:0000:0000:0000:0000/128",
        new byte[] { 32, 1, 13, 184, 172, 16, 254, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
        128)]
    public void DecodeText_Should_DecodeTextEncodedValueAsIPNetwork(
        string textData,
        byte[] address,
        byte prefixLength)
    {
        var expectedValue = new IPNetwork(new IPAddress(address), prefixLength);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        IPNetwork actualValue = PgIpNetwork.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(
        "error",
        "Could not parse 'error' into a network value")]
    [InlineData(
        "192.168.0.0/error",
        "Could not parse '192.168.0.0/error' into a network value")]
    public void DecodeText_Should_Fail_When_InvalidIPNetwork(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            IPNetwork value = PgIpNetwork.DecodeText(textValue);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Net.IPNetwork", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (ArgumentException e)
        {
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnIpNetworkType() =>
        Assert.Equal(PgIpNetwork.DbType, PgTypeInfo.Cidr);

    [Fact]
    public void ArrayDbType_Should_ReturnIpNetworkType() =>
        Assert.Equal(PgIpNetwork.ArrayDbType, PgTypeInfo.CidrArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgIpNetwork.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Cidr, true];
        yield return [PgTypeInfo.Inet, true];
        yield return [PgTypeInfo.CidrArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
