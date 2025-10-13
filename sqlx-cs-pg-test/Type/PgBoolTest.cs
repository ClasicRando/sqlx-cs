using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBool))]
public class PgBoolTest
{
    [Theory]
    [InlineData(true, new byte[] { 1 })]
    [InlineData(false, new byte[] { 0 })]
    public void Encode_Should_WriteByte(bool value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgBool.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(new byte[] { 1 }, true)]
    [InlineData(new byte[] { 255 }, true)]
    [InlineData(new byte[] { 0 }, false)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsBoolean(
        byte[] binaryData,
        bool expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgBool.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("t", true)]
    [InlineData("true", true)]
    [InlineData("f", false)]
    public void DecodeText_Should_DecodeTextEncodedValueAsBoolean(
        string textData,
        bool expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgBool.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_FirstCharacterIsNotValid(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgBool.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Boolean", e.Message);
            Assert.Contains("First character must be 't' or 'f'", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnBoolType() => Assert.Equal(PgBool.DbType, PgType.Bool);

    [Fact]
    public void ArrayDbType_Should_ReturnBoolArrayType() =>
        Assert.Equal(PgBool.ArrayDbType, PgType.BoolArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgBool.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Bool, true];
        yield return [PgType.BoolArray, false];
        yield return [PgType.Int4, false];
    }

    [Theory]
    [MemberData(nameof(GetActualTypeCases))]
    public void GetActualType(bool value, PgType expectedResult) =>
        Assert.Equal(expectedResult, PgBool.GetActualType(value));

    public static IEnumerable<object[]> GetActualTypeCases()
    {
        yield return [true, PgType.Bool];
        yield return [false, PgType.Bool];
    }
}
