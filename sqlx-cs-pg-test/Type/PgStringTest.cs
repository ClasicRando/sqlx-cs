using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgString))]
public class PgStringTest
{
    [Theory]
    [InlineData(
        "This is a test",
        new byte[] { 84, 104, 105, 115, 32, 105, 115, 32, 97, 32, 116, 101, 115, 116 })]
    [InlineData("😀", new byte[] { 240, 159, 152, 128 })]
    public void Encode_Should_WriteText(string value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgString.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[] { 84, 104, 105, 115, 32, 105, 115, 32, 97, 32, 116, 101, 115, 116 },
        "This is a test")]
    [InlineData(new byte[] { 240, 159, 152, 128 }, "😀")]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsText(
        byte[] binaryData,
        string expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgString.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("This is a test", "This is a test")]
    [InlineData("😀", "😀")]
    public void DecodeText_Should_DecodeTextEncodedValueAsText(
        string textData,
        string expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgString.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DbType_Should_ReturnTextType() => Assert.Equal(PgString.DbType, PgType.Text);

    [Fact]
    public void ArrayDbType_Should_ReturnTextType() =>
        Assert.Equal(PgString.ArrayDbType, PgType.TextArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgString.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Text, true];
        yield return [PgType.Varchar, true];
        yield return [PgType.Xml, true];
        yield return [PgType.Name, true];
        yield return [PgType.Bpchar, true];
        yield return [PgType.TextArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Text, PgString.GetActualType(string.Empty));
    }
}
