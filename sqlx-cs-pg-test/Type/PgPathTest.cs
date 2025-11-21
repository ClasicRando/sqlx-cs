using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPath))]
public class PgPathTest
{
    [Theory]
    [MemberData(nameof(EncodeCases))]
    public void Encode_Should_WritePath(PgPath value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgPath.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    public static IEnumerable<TheoryDataRow<PgPath, byte[]>> EncodeCases()
    {
        return new TheoryData<PgPath, byte[]>(
            (new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]), [0, 0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102]),
            (new PgPath(true, [new PgPoint(4.87, 2.8)]), [1, 0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102]));
    }

    [Theory]
    [MemberData(nameof(DecodeBytesCases))]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsPath(
        byte[] binaryData,
        PgPath expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgPath actualValue = PgPath.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    public static IEnumerable<TheoryDataRow<byte[], PgPath>> DecodeBytesCases()
    {
        return new TheoryData<byte[], PgPath>(
            ([0, 0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102], new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)])),
            ([1, 0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102], new PgPath(true, [new PgPoint(4.87, 2.8)])));
    }

    [Theory]
    [MemberData(nameof(DecodeTextCases))]
    public void DecodeText_Should_DecodeTextEncodedValueAsPath(
        string textData,
        PgPath expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgPath actualValue = PgPath.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    public static IEnumerable<TheoryDataRow<string, PgPath>> DecodeTextCases()
    {
        return new TheoryData<string, PgPath>(
            ("[(5.63,8.59),(4.87,2.8)]", new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)])),
            ("((4.87, 2.8))", new PgPath(true, [new PgPoint(4.87, 2.8)])));
    }

    [Fact]
    public void DbType_Should_ReturnPathType() => Assert.Equal(PgPath.DbType, PgTypeInfo.Path);

    [Fact]
    public void ArrayDbType_Should_ReturnPathType() =>
        Assert.Equal(PgPath.ArrayDbType, PgTypeInfo.PathArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgPath.IsCompatible(pgType));

    public static IEnumerable<TheoryDataRow<PgTypeInfo, bool>> IsCompatibleCases()
    {
        return new TheoryData<PgTypeInfo, bool>(
            (PgTypeInfo.Path, true),
            (PgTypeInfo.PathArray, false),
            (PgTypeInfo.Int4, false));
    }
}
