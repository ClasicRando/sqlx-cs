using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgOid))]
public class PgOidTest
{
    [Theory]
    [InlineData(0, new byte[] { 0, 0, 0, 0 })]
    [InlineData(short.MaxValue, new byte[] { 0, 0, 127, 255 })]
    [InlineData(int.MaxValue, new byte[] { 127, 255, 255, 255 })]
    [InlineData(uint.MaxValue, new byte[] { 255, 255, 255, 255 })]
    public void Encode_Should_WriteInt(uint value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgOid.Encode(new PgOid(value), buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [InlineData(new byte[] { 127, 255 }, short.MaxValue)]
    [InlineData(new byte[] { 0, 0, 127, 255 }, short.MaxValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 }, short.MaxValue)]
    [InlineData(new byte[] { 127, 255, 255, 255 }, int.MaxValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 }, int.MaxValue)]
    [InlineData(new byte[] { 255, 255, 255, 255 }, uint.MaxValue)]
    [InlineData(new byte[] { 0, 0, 0, 0, 255, 255, 255, 255 }, uint.MaxValue)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsInt(
        byte[] binaryData,
        uint expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgOid actualValue = PgOid.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue.Inner);
    }

    [Theory]
    [InlineData(new byte[] { 255, 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 })]
    public void DecodeBytes_Should_Fail_When_OutsideOfIntBounds(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);
        try
        {
            PgOid temp = PgOid.DecodeBytes(ref binaryValue);
            Assert.Fail($"Decoding should have failed. Found '{temp}'");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.UInt32", e.Message);
            Assert.Contains("Value is outside of valid uint", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData(new byte[] { 0 })]
    [InlineData(new byte[] { 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public void DecodeBytes_Should_Fail_When_InvalidNumberOfBytes(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);
        try
        {
            PgOid.DecodeBytes(ref binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.UInt32", e.Message);
            Assert.Contains("Could not extract integer from buffer. Number of bytes = ", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("32767", short.MaxValue)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("4294967295", uint.MaxValue)]
    public void DecodeText_Should_DecodeTextEncodedValueAsInt(
        string textData,
        uint expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgOid actualValue = PgOid.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue.Inner);
    }

    [Theory]
    [InlineData("error", "Could not convert 'error' into System.UInt32")]
    [InlineData("-9223372036854775808", "Value is outside of valid uint")]
    [InlineData("9223372036854775807", "Value is outside of valid uint")]
    public void DecodeText_Should_Fail_When_InvalidIntString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgOid.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.UInt32", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnIntType() => Assert.Equal(PgTypeInfo.Oid, PgOid.DbType);

    [Fact]
    public void ArrayDbType_Should_ReturnIntType() =>
        Assert.Equal(PgTypeInfo.OidArray, PgOid.ArrayDbType);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgInt.IsCompatible(pgType));

    public static IEnumerable<TheoryDataRow<PgTypeInfo, bool>> IsCompatibleCases()
    {
        return new TheoryData<PgTypeInfo, bool>(
            (PgTypeInfo.Int8, true),
            (PgTypeInfo.OidArray, false),
            (PgTypeInfo.Oid, true),
            (PgTypeInfo.Int4, true),
            (PgTypeInfo.Int2, true));
    }
}
