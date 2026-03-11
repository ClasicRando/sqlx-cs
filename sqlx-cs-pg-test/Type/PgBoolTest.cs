using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBool))]
public class PgBoolTest
{
    [Test]
    [Arguments(true, new byte[] { 1 })]
    [Arguments(false, new byte[] { 0 })]
    public async Task Encode_Should_WriteByte(bool value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgBool.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 1 }, true)]
    [Arguments(new byte[] { 255 }, true)]
    [Arguments(new byte[] { 0 }, false)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsBoolean(
        byte[] binaryData,
        bool expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        var actualValue = PgBool.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("t", true)]
    [Arguments("true", true)]
    [Arguments("f", false)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsBoolean(
        string textData,
        bool expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        var actualValue = PgBool.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_FirstCharacterIsNotValid(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgBool.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Boolean");
            await Assert.That(e.Message).Contains("First character must be 't' or 'f'");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnBoolType() => await Assert.That(PgBool.DbType).IsEqualTo(PgTypeInfo.Bool);

    [Test]
    public async Task ArrayDbType_Should_ReturnBoolArrayType() =>
        await Assert.That(PgBool.ArrayDbType).IsEqualTo(PgTypeInfo.BoolArray);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgBool.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Bool, true);
        yield return () => (PgTypeInfo.BoolArray, false);
        yield return () => (PgTypeInfo.Int4, false);
    }
}
