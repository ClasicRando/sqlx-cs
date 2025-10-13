using JetBrains.Annotations;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBytea))]
public class PgByteaTest
{
    [Theory]
    [InlineData("\\xdeadbeef", new byte[] { 0xde, 0xad, 0xbe, 0xef })]
    [InlineData(@"\000\047\134", new byte[] { 0x00, 0x27, 0x5c })]
    [InlineData(@"'\\", new byte[] { 0x27, 0x5c })]
    public void DecodeText_Should_DecodeTextEncodedValueAsBytes(
        string textData,
        byte[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgBytea.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("\\xdea", "Hex encoded byte array must have an even number of elements")]
    public void DecodeText_Should_Fail_When_FirstCharacterIsNotValid(
        string textData,
        string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgBytea.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Byte[]", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnByteaType() => Assert.Equal(PgBytea.DbType, PgType.Bytea);

    [Fact]
    public void ArrayDbType_Should_ReturnByteaType() =>
        Assert.Equal(PgBytea.ArrayDbType, PgType.ByteaArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgBytea.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Bytea, true];
        yield return [PgType.ByteaArray, false];
        yield return [PgType.Int4, false];
    }

    [Theory]
    [MemberData(nameof(GetActualTypeCases))]
    public void GetActualType(byte[] value, PgType expectedResult) =>
        Assert.Equal(expectedResult, PgBytea.GetActualType(value));

    public static IEnumerable<object[]> GetActualTypeCases()
    {
        yield return [Array.Empty<byte>(), PgType.Bytea];
        yield return [new byte[] { 1, 2, 3 }, PgType.Bytea];
    }
}
