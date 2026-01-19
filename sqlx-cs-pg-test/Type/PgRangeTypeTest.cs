using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgRangeType<,>))]
public class PgRangeTypeTest
{
    [Test]
    [MethodDataSource(nameof(EncodeTestCases))]
    public async Task Encode_Should_WriteIntRange(PgRange<int> value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgRangeType<int, PgInt>.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    public static IEnumerable<Func<(PgRange<int>, byte[])>> EncodeTestCases()
    {
        yield return () => (new PgRange<int>(Bound.Included(-1), Bound.Excluded(11)), [0x02, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B]);
        yield return () => (new PgRange<int>(Bound.Excluded(-1), Bound.Included(11)), [0x04, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B,]);
        yield return () => (new PgRange<int>(Bound.Excluded(-1), Bound.Unbounded<int>()), [0x10, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF]);
        yield return () => (new PgRange<int>(Bound.Unbounded<int>(), Bound.Included(11)), [0x08 | 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B]);
    }

    [Test]
    [MethodDataSource(nameof(DecodeBytesTestCases))]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsIntRange(
        byte[] binaryData,
        PgRange<int> expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        var actualValue = PgRangeType<int, PgInt>.DecodeBytes(ref binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    public static IEnumerable<Func<(byte[], PgRange<int>)>> DecodeBytesTestCases()
    {
        yield return () => ([0x02, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B], new PgRange<int>(Bound.Included(-1), Bound.Excluded(11)));
        yield return () => ([0x04, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B], new PgRange<int>(Bound.Excluded(-1), Bound.Included(11)));
        yield return () => ([0x10, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF], new PgRange<int>(Bound.Excluded(-1), Bound.Unbounded<int>()));
        yield return () => ([0x08 | 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B], new PgRange<int>(Bound.Unbounded<int>(), Bound.Included(11)));
    }

    [Test]
    [MethodDataSource(nameof(DecodeTextTestCases))]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsIntRange(
        string textData,
        PgRange<int> expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        var actualValue = PgRangeType<int, PgInt>.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    public static IEnumerable<Func<(string, PgRange<int>)>> DecodeTextTestCases()
    {
        yield return () => ("[-1,11)", new PgRange<int>(Bound.Included(-1), Bound.Excluded(11)));
        yield return () => ("(-1,11]", new PgRange<int>(Bound.Excluded(-1), Bound.Included(11)));
        yield return () => ("(-1,)", new PgRange<int>(Bound.Excluded(-1), Bound.Unbounded<int>()));
        yield return () => ("(,11]", new PgRange<int>(Bound.Unbounded<int>(), Bound.Included(11)));
    }

    [Test]
    [Arguments("error")]
    public async Task DecodeText_Should_Fail_When_InvalidArrayLiteral(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgRangeType<int, PgInt>.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: Sqlx.Postgres.Type.PgRange");
            await Assert.That(e.Message).Contains("Could not find separator character in ");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnRangeType() => await Assert.That(PgTypeInfo.Int4Range).IsEqualTo(PgRangeType<int, PgInt>.DbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) => await Assert.That(PgRangeType<int, PgInt>.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Int4Range, true);
        yield return () => (PgTypeInfo.Text, false);
        yield return () => (PgTypeInfo.Int4RangeArray, false);
    }
}
