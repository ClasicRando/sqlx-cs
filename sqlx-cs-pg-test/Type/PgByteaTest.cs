using JetBrains.Annotations;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBytea))]
public class PgByteaTest
{
    [Test]
    [Arguments("\\xdeadbeef", new byte[] { 0xde, 0xad, 0xbe, 0xef })]
    [Arguments(@"\000\047\134", new byte[] { 0x00, 0x27, 0x5c })]
    [Arguments(@"'\\", new byte[] { 0x27, 0x5c })]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsBytes(
        string textData,
        byte[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgBytea.DecodeText(textValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    [Test]
    [Arguments("\\xdea", "Hex encoded byte array must have an even number of elements")]
    public async Task DecodeText_Should_Fail_When_FirstCharacterIsNotValid(
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
            await Assert.That(e.Message).Contains("Desired Output: System.Byte[]");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnByteaType() => await Assert.That(PgTypeInfo.Bytea).IsEqualTo(PgBytea.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnByteaType() =>
        await Assert.That(PgTypeInfo.ByteaArray).IsEqualTo(PgBytea.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgBytea.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Bytea, true];
        yield return [PgTypeInfo.ByteaArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
