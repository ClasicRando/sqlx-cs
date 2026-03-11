using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgString))]
public class PgStringTest
{
    [Test]
    [Arguments(
        "This is a test",
        new byte[] { 84, 104, 105, 115, 32, 105, 115, 32, 97, 32, 116, 101, 115, 116 })]
    [Arguments("😀", new byte[] { 240, 159, 152, 128 })]
    public async Task Encode_Should_WriteText(string value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgString.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(
        new byte[] { 84, 104, 105, 115, 32, 105, 115, 32, 97, 32, 116, 101, 115, 116 },
        "This is a test")]
    [Arguments(new byte[] { 240, 159, 152, 128 }, "😀")]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsText(
        byte[] binaryData,
        string expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        var actualValue = PgString.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("This is a test", "This is a test")]
    [Arguments("😀", "😀")]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsText(
        string textData,
        string expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        var actualValue = PgString.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task DbType_Should_ReturnTextType() => await Assert.That(PgTypeInfo.Text).IsEqualTo(PgString.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnTextType() =>
        await Assert.That(PgTypeInfo.TextArray).IsEqualTo(PgString.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgString.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Text, true];
        yield return [PgTypeInfo.Varchar, true];
        yield return [PgTypeInfo.Xml, true];
        yield return [PgTypeInfo.Name, true];
        yield return [PgTypeInfo.Bpchar, true];
        yield return [PgTypeInfo.TextArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
