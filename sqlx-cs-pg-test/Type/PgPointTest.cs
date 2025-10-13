using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgPoint))]
public class PgPointTest
{
    [Fact]
    public void Encode_Should_WritePoint()
    {
        byte[] expectedBytes =
            [64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174];
        var value = new PgPoint(5.63, 8.59);
        using var buffer = new WriteBuffer();

        PgPoint.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Fact]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsPoint()
    {
        byte[] binaryData =
            [64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174];
        var expectedValue = new PgPoint(5.63, 8.59);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgPoint actualValue = PgPoint.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DecodeText_Should_DecodeTextEncodedValueAsPoint()
    {
        const string textData = "(5.63,8.59)";
        var expectedValue = new PgPoint(5.63, 8.59);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgPoint actualValue = PgPoint.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("(error)", "Could not find point separator character")]
    [InlineData("(error,1)", "Could not parse X coordinate")]
    [InlineData("(1,error)", "Could not parse Y coordinate")]
    public void DecodeText_Should_Fail_When_InvalidText(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgPoint.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgPoint", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnPointType() => Assert.Equal(PgPoint.DbType, PgType.Point);

    [Fact]
    public void ArrayDbType_Should_ReturnPointType() =>
        Assert.Equal(PgPoint.ArrayDbType, PgType.PointArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgPoint.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Point, true];
        yield return [PgType.PointArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        var value = new PgPoint(0, 0);
        Assert.Equal(PgType.Point, PgPoint.GetActualType(value));
    }
}
