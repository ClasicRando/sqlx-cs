using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPath))]
public class PgPathTest
{
    [Theory]
    [InlineData(
        false,
        new[] { 5.63, 8.59, 4.87, 2.8 },
        new byte[]
        {
            0, 0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174,
            64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        })]
    [InlineData(
        true,
        new[] { 4.87, 2.8 },
        new byte[]
        {
            1, 0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        })]
    public void Encode_Should_WritePath(
        bool isClosed,
        double[] values,
        byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var value = new PgPath(isClosed, points);

        PgPath.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[]
        {
            0, 0, 0, 0, 2, 64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174,
            64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        },
        false,
        new[] { 5.63, 8.59, 4.87, 2.8 })]
    [InlineData(
        new byte[]
        {
            1, 0, 0, 0, 1, 64, 19, 122, 225, 71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        },
        true,
        new[] { 4.87, 2.8 })]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsPath(
        byte[] binaryData,
        bool isClosed,
        double[] values)
    {
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var expectedValue = new PgPath(isClosed, points);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgPath actualValue = PgPath.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData(
        "[(5.63,8.59),(4.87,2.8)]",
        false,
        new[] { 5.63, 8.59, 4.87, 2.8 })]
    [InlineData(
        "((5.63,8.59))",
        true,
        new[] { 5.63, 8.59 })]
    public void DecodeText_Should_DecodeTextEncodedValueAsPath(
        string textData,
        bool isClosed,
        double[] values)
    {
        var points = new PgPoint[values.Length / 2];
        var j = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var x = values[j++];
            var y = values[j++];
            points[i] = new PgPoint(x, y);
        }

        var expectedValue = new PgPath(isClosed, points);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgPath actualValue = PgPath.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DbType_Should_ReturnPathType() => Assert.Equal(PgPath.DbType, PgType.Path);

    [Fact]
    public void ArrayDbType_Should_ReturnPathType() =>
        Assert.Equal(PgPath.ArrayDbType, PgType.PathArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgPath.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Path, true];
        yield return [PgType.PathArray, false];
        yield return [PgType.Int4, false];
    }


    [Fact]
    public void GetActualType()
    {
        var value = new PgPath();
        Assert.Equal(PgType.Path, PgPath.GetActualType(value));
    }
}
