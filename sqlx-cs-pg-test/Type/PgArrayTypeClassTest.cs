using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgArrayTypeClass<,>))]
public class PgArrayTypeClassTest
{
    [Test]
    [Arguments(
        new string[] { },
        new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 0, 0, 0, 0, 1 })]
    [Arguments(
        new[] { "test" },
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, (byte)'t',
            (byte)'e', (byte)'s', (byte)'t',
        })]
    [Arguments(
        new[] { null, "test" },
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0,
            0, 4, (byte)'t', (byte)'e', (byte)'s', (byte)'t',
        })]
    public async Task Encode_Should_WriteStringArray(string[] value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgArrayTypeClass<string, PgString>.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(
        new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 0, 0, 0, 0, 1 },
        new string[] { })]
    [Arguments(
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, (byte)'t',
            (byte)'e', (byte)'s', (byte)'t',
        },
        new[] { "test" })]
    [Arguments(
        new byte[]
        {
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 25, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0,
            0, 4, (byte)'t', (byte)'e', (byte)'s', (byte)'t',
        },
        new[] { null, "test" })]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsStringArray(
        byte[] binaryData,
        string?[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgArrayTypeClass<string, PgString>.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    [Test]
    [Arguments("{}", new string[] { })]
    [Arguments("{test}", new[] { "test" })]
    [Arguments("{,test}", new[] { "", "test" })]
    [Arguments("{,\"test\"}", new[] { "", "test" })]
    [Arguments("{\"\",test}", new[] { "", "test" })]
    [Arguments("{NULL,\"test\"}", new[] { null, "test" })]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsStringArray(
        string textData,
        string?[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgArrayTypeClass<string, PgString>.DecodeText(textValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_InvalidArrayLiteral(string textData)
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
            await Assert.That(e.Message).Contains("Desired Output: System.String[]");
            await Assert.That(e.Message).Contains("Array literal must be enclosed in curly braces");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnArrayType() => await Assert.That(PgTypeInfo.TextArray)
        .IsEqualTo(PgArrayTypeClass<string, PgString>.DbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) => await Assert
        .That(PgArrayTypeClass<string, PgString>.IsCompatible(pgType))
        .IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.TextArray, true);
        yield return () => (PgTypeInfo.Text, false);
        yield return () => (PgTypeInfo.Int4Array, false);
    }
}
