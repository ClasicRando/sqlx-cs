using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgMacAddress))]
public class PgMacAddressTest
{
    [Theory]
    [InlineData(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03 })]
    [InlineData(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public void Encode_Should_WriteMacAddr(byte[] address)
    {
        PgMacAddress value = PgMacAddress.FromBytes(address);
        using var buffer = new WriteBuffer();

        PgMacAddress.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(address, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03 })]
    [InlineData(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsMacAddr(byte[] binaryData)
    {
        PgMacAddress expectedValue = PgMacAddress.FromBytes(binaryData);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgMacAddress actualValue = PgMacAddress.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("08:00:2b:01:02:03", new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03 })]
    [InlineData(
        "ccccccccccccccccccc",
        new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public void DecodeText_Should_DecodeTextEncodedValueAsMacAddr(string textData, byte[] address)
    {
        PgMacAddress expectedValue = PgMacAddress.FromBytes(address);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgMacAddress actualValue = PgMacAddress.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("01:01:01:01:01", "Expected 6 or 8 address hex characters")]
    [InlineData("01:01:01:01:1:01", "Could not parse network location bytes from")]
    public void DecodeText_Should_Fail_When_InvalidPgMacAddress(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgMacAddress value = PgMacAddress.DecodeText(textValue);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgMacAddress", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnMacAddrType() =>
        Assert.Equal(PgMacAddress.DbType, PgType.Macaddr);

    [Fact]
    public void ArrayDbType_Should_ReturnMacAddrType() =>
        Assert.Equal(PgMacAddress.ArrayDbType, PgType.MacaddrArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgMacAddress.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Macaddr, true];
        yield return [PgType.Macaddr8, true];
        yield return [PgType.Macaddr8Array, false];
        yield return [PgType.Int4, false];
    }

    [Theory]
    [InlineData(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }, true)]
    [InlineData(new byte[] { 1, 1, 1, 1, 1, 1 }, false)]
    public void GetActualType(byte[] address, bool isMacAddr8)
    {
        Assert.Equal(
            isMacAddr8 ? PgType.Macaddr8 : PgType.Macaddr,
            PgMacAddress.GetActualType(PgMacAddress.FromBytes(address)));
    }
}
