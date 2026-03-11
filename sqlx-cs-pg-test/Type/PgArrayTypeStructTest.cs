using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgArrayTypeStruct<,>))]
public class PgArrayTypeStructTest
{
    [Test]
    [MethodDataSource(nameof(EncodeCases))]
    public async Task Encode_Should_WriteIntArray(int?[] value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgArrayTypeStruct<int, PgInt>.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    public static IEnumerable<Func<(int?[], byte[])>> EncodeCases()
    {
        yield return () => ([], [0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 0, 0, 0, 0, 1]);
        yield return () => ([1],
            [0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 1]);
        yield return () => ([null, 1],
        [
            0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0,
            0, 4, 0, 0, 0, 1,
        ]);
    }

    [Test]
    [MethodDataSource(nameof(DecodeBytesCases))]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsIntArray(
        byte[] binaryData,
        int?[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        var actualValue = PgArrayTypeStruct<int, PgInt>.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    public static IEnumerable<object[]> DecodeBytesCases()
    {
        yield return
        [
            new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 0, 0, 0, 0, 1 },
            Array.Empty<int?>(),
        ];
        yield return
        [
            new byte[]
            {
                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 1
            },
            new int?[] { 1 },
        ];
        yield return
        [
            new byte[]
            {
                0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0,
                0, 0, 4, 0, 0, 0, 1,
            },
            new int?[] { null, 1 },
        ];
    }

    [Test]
    [MethodDataSource(nameof(DecodeTextCases))]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsIntArray(
        string textData,
        int?[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        var actualValue = PgArrayTypeStruct<int, PgInt>.DecodeText(textValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    public static IEnumerable<object[]> DecodeTextCases()
    {
        yield return ["{}", Array.Empty<int?>()];
        yield return ["{1}", new int?[] { 1 }];
        yield return ["{NULL,1}", new int?[] { null, 1 }];
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_InvalidArrayLiteral(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgArrayTypeStruct<int, PgInt>.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Int32[]");
            await Assert.That(e.Message).Contains("Array literal must be enclosed in curly braces");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnArrayType() => await Assert.That(PgTypeInfo.Int4Array)
        .IsEqualTo(PgArrayTypeStruct<int, PgInt>.DbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) => await Assert
        .That(PgArrayTypeStruct<int, PgInt>.IsCompatible(pgType))
        .IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Int4Array, true);
        yield return () => (PgTypeInfo.Int4, false);
        yield return () => (PgTypeInfo.TextArray, false);
    }
}
