using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgRangeType<,>))]
public class PgRangeTypeTest
{
    [Theory]
    [MemberData(nameof(EncodeTestCases))]
    public void Encode_Should_WriteIntRange(PgRange<int> value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgRangeType<int, PgInt>.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    public static IEnumerable<TheoryDataRow<PgRange<int>, byte[]>> EncodeTestCases()
    {
        return new TheoryData<PgRange<int>, byte[]>(
            (new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11)),
            [
                0x02, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x00, 0x00, 0x0B,
            ]),
            (new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Included(11)),
            [
                0x04, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00,
                0x00, 0x00, 0x0B,
            ]),
            (new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Unbounded()),
                [0x10, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF]),
            (new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Included(11)),
                [0x08 | 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B]));
    }

    [Theory]
    [MemberData(nameof(DecodeBytesTestCases))]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsIntRange(
        byte[] binaryData,
        PgRange<int> expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgRangeType<int, PgInt>.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    public static IEnumerable<TheoryDataRow<byte[], PgRange<int>>> DecodeBytesTestCases()
    {
        return new TheoryData<byte[], PgRange<int>>(
            ([0x02, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B],
                new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11))),
            ([0x04, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B],
                new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Included(11))),
            ([0x10, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF],
                new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Unbounded())),
            ([0x08 | 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B],
                new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Included(11))));
    }

    [Theory]
    [MemberData(nameof(DecodeTextTestCases))]
    public void DecodeText_Should_DecodeTextEncodedValueAsIntRange(
        string textData,
        PgRange<int> expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgRangeType<int, PgInt>.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    public static IEnumerable<TheoryDataRow<string, PgRange<int>>> DecodeTextTestCases()
    {
        return new TheoryData<string, PgRange<int>>(
            ("[-1,11)", new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11))),
            ("(-1,11]", new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Included(11))),
            ("(-1,)", new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Unbounded())),
            ("(,11]", new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Included(11))));
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_InvalidArrayLiteral(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgRangeType<int, PgInt>.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgRange", e.Message);
            Assert.Contains("Could not find separator character in ", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnRangeType() => Assert.Equal(
        PgRangeType<int, PgInt>.DbType,
        PgType.Int4Range);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) => Assert.Equal(
        expectedResult,
        PgRangeType<int, PgInt>.IsCompatible(pgType));

    public static IEnumerable<TheoryDataRow<PgType, bool>> IsCompatibleCases()
    {
        return new TheoryData<PgType, bool>(
            (PgType.Int4Range, true),
            (PgType.Text, false),
            (PgType.Int4RangeArray, false));
    }

    [Fact]
    public void GetActualType()
    {
        var value = new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Unbounded());
        Assert.Equal(PgType.Int4Range, PgRangeType<int, PgInt>.GetActualType(value));
    }
}
