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

    public static IEnumerable<object[]> EncodeTestCases()
    {
        yield return [new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11)), new byte[] { 0x02, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B }];
        yield return [new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Included(11)), new byte[] { 0x04, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B }];
        yield return [new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Unbounded()), new byte[] { 0x10, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF }];
        yield return [new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Included(11)), new byte[] { 0x08 | 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B }];
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

    public static IEnumerable<object[]> DecodeBytesTestCases()
    {
        yield return [new byte[] { 0x02, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B }, new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11))];
        yield return [new byte[] { 0x04, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B }, new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Included(11))];
        yield return [new byte[] { 0x10, 0x00, 0x00, 0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF }, new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Unbounded())];
        yield return [new byte[] { 0x08 | 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0B }, new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Included(11))];
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

    public static IEnumerable<object[]> DecodeTextTestCases()
    {
        yield return ["[-1,11)", new PgRange<int>(Bound<int>.Included(-1), Bound<int>.Excluded(11))];
        yield return ["(-1,11]", new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Included(11))];
        yield return ["(-1,)", new PgRange<int>(Bound<int>.Excluded(-1), Bound<int>.Unbounded())];
        yield return ["(,11]", new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Included(11))];
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

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Int4Range, true];
        yield return [PgType.Text, false];
        yield return [PgType.Int4RangeArray, false];
    }

    [Fact]
    public void GetActualType()
    {
        var value = new PgRange<int>(Bound<int>.Unbounded(), Bound<int>.Unbounded());
        Assert.Equal(PgType.Int4Range, PgRangeType<int, PgInt>.GetActualType(value));
    }
}
