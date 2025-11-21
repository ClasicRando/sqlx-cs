using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgChar))]
public class PgCharTest
{
    [Theory]
    [InlineData(sbyte.MinValue, new byte[] { 128 })]
    [InlineData(1, new byte[] { 1 })]
    [InlineData(sbyte.MaxValue, new byte[] { 127 })]
    public void Encode_Should_WriteByte(sbyte value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgChar.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 128 }, sbyte.MinValue)]
    [InlineData(new byte[] { 1 }, 1)]
    [InlineData(new byte[] { 127 }, sbyte.MaxValue)]
    [InlineData(new byte[] { }, 0)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsSbyte(
        byte[] binaryData,
        sbyte expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgChar.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("t", 116)]
    [InlineData("\\147", 103)]
    public void DecodeText_Should_DecodeTextEncodedValueAsSbyte(
        string textData,
        sbyte expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgChar.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("ex")]
    [InlineData("err")]
    [InlineData("error")]
    [InlineData("error test")]
    public void DecodeText_Should_Fail_When_InvalidNumberOfCharacters(string textData)
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
            Assert.Contains("Desired Output: System.SByte", e.Message);
            Assert.Contains("Received invalid \"char\" text", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnCharType() => Assert.Equal(PgChar.DbType, PgTypeInfo.Char);

    [Fact]
    public void ArrayDbType_Should_ReturnCharType() =>
        Assert.Equal(PgChar.ArrayDbType, PgTypeInfo.CharArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgChar.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Char, true];
        yield return [PgTypeInfo.CharArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
