using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgMacAddress8))]
public class PgMacAddress8Test
{
    [Theory]
    [InlineData(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public void Encode_Should_WriteMacAddr8(byte[] address)
    {
        PgMacAddress8 value = PgMacAddress8.FromBytes(address);
        using var buffer = new WriteBuffer();

        PgMacAddress8.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(address, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsMacAddr8(byte[] binaryData)
    {
        PgMacAddress8 expectedValue = PgMacAddress8.FromBytes(binaryData);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgMacAddress8 actualValue = PgMacAddress8.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(
        "08:00:2b:01:02:03:04:05",
        new byte[] { 0x08, 0x00, 0x2b, 0x01, 0x02, 0x03, 0x04, 0x05 })]
    public void DecodeText_Should_DecodeTextEncodedValueAsMacAddr8(string textData, byte[] address)
    {
        PgMacAddress8 expectedValue = PgMacAddress8.FromBytes(address);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgMacAddress8 actualValue = PgMacAddress8.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("01:01:01:01:01", "Expected 8 address hex characters")]
    [InlineData("01:01:01:01:1:01:01:01", "Could not parse network location bytes from")]
    public void DecodeText_Should_Fail_When_InvalidPgMacAddress8(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgMacAddress8 value = PgMacAddress8.DecodeText(textValue);
            Assert.Fail($"Decoding should have failed. Found '{value}'");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgMacAddress8", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnMacAddr8Type() =>
        Assert.Equal(PgTypeInfo.Macaddr8, PgMacAddress8.DbType);

    [Fact]
    public void ArrayDbType_Should_ReturnMacAddr8Type() =>
        Assert.Equal(PgTypeInfo.Macaddr8Array, PgMacAddress8.ArrayDbType);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgMacAddress8.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Macaddr8, true];
        yield return [PgTypeInfo.Macaddr8Array, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
