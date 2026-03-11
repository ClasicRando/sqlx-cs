using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPath))]
public class PgPathTest
{
    [Test]
    [MethodDataSource(nameof(EncodeCases))]
    public async Task Encode_Should_WritePath(PgPath value, byte[] expectedBytes)
    {
        using var buffer = new PooledArrayBufferWriter();

        PgPath.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    public static IEnumerable<Func<(PgPath, byte[])>> EncodeCases()
    {
        yield return () => (new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]), [0, 0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102]);
        yield return () => (new PgPath(true, [new PgPoint(4.87, 2.8)]), [1, 0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102]);
    }

    [Test]
    [MethodDataSource(nameof(DecodeBytesCases))]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsPath(
        byte[] binaryData,
        PgPath expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgPath actualValue = PgPath.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    public static IEnumerable<Func<(byte[], PgPath)>> DecodeBytesCases()
    {
        yield return () => ([0, 0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102], new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]));
        yield return () => ([1, 0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102], new PgPath(true, [new PgPoint(4.87, 2.8)]));
    }

    [Test]
    [MethodDataSource(nameof(DecodeTextCases))]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsPath(
        string textData,
        PgPath expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgPath actualValue = PgPath.DecodeText(textValue);

        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    public static IEnumerable<Func<(string, PgPath)>> DecodeTextCases()
    {
        yield return () => ("[(5.63,8.59),(4.87,2.8)]", new PgPath(false, [new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8)]));
        yield return () => ("((4.87, 2.8))", new PgPath(true, [new PgPoint(4.87, 2.8)]));
    }

    [Test]
    public async Task DbType_Should_ReturnPathType() => await Assert.That(PgTypeInfo.Path).IsEqualTo(PgPath.DbType);

    [Test]
    public async Task ArrayDbType_Should_ReturnPathType() =>
        await Assert.That(PgTypeInfo.PathArray).IsEqualTo(PgPath.ArrayDbType);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgPath.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Path, true);
        yield return () => (PgTypeInfo.PathArray, false);
        yield return () => (PgTypeInfo.Int4, false);
    }
}
