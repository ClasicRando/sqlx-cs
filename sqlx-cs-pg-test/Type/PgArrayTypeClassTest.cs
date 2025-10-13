using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgArrayTypeClass<,>))]
public class PgArrayTypeClassTest
{
    [Theory]
    [InlineData(
        new string[] { },
        new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 0, 0, 0, 0, 1 })]
    [InlineData(
        new[] { "test" },
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, (byte)'t',
            (byte)'e', (byte)'s', (byte)'t',
        })]
    [InlineData(
        new[] { null, "test" },
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0,
            0, 4, (byte)'t', (byte)'e', (byte)'s', (byte)'t',
        })]
    public void Encode_Should_WriteStringArray(string[] value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgArrayTypeClass<string, PgString>.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 0, 0, 0, 0, 1 },
        new string[] { })]
    [InlineData(
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, (byte)'t',
            (byte)'e', (byte)'s', (byte)'t',
        },
        new[] { "test" })]
    [InlineData(
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0,
            0, 4, (byte)'t', (byte)'e', (byte)'s', (byte)'t',
        },
        new[] { null, "test" })]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsStringArray(
        byte[] binaryData,
        string[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgArrayTypeClass<string, PgString>.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("{}", new string[] { })]
    [InlineData("{test}", new[] { "test" })]
    [InlineData("{,test}", new[] { "", "test" })]
    [InlineData("{,\"test\"}", new[] { "", "test" })]
    [InlineData("{\"\",test}", new[] { "", "test" })]
    [InlineData("{NULL,\"test\"}", new[] { null, "test" })]
    public void DecodeText_Should_DecodeTextEncodedValueAsStringArray(
        string textData,
        string[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgArrayTypeClass<string, PgString>.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_InvalidArrayLiteral(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgArrayTypeClass<string, PgString>.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.String[]", e.Message);
            Assert.Contains("Array literal must be enclosed in curly braces", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnArrayType() => Assert.Equal(
        PgArrayTypeClass<string, PgString>.DbType,
        PgType.TextArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) => Assert.Equal(
        expectedResult,
        PgArrayTypeClass<string, PgString>.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.TextArray, true];
        yield return [PgType.Text, false];
        yield return [PgType.Int4Array, false];
    }

    [Theory]
    [MemberData(nameof(GetActualTypeCases))]
    public void GetActualType(string[] value, PgType expectedResult) => Assert.Equal(
        expectedResult,
        PgArrayTypeClass<string, PgString>.GetActualType(value));

    public static IEnumerable<object[]> GetActualTypeCases()
    {
        yield return [Array.Empty<string>(), PgType.TextArray];
        yield return [new[] { "test" }, PgType.TextArray];
    }
}
