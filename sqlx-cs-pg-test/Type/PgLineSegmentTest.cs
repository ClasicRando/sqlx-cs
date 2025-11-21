using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgLineSegment))]
public class PgLineSegmentTest
{
    [Fact]
    public void Encode_Should_WritePgCircle()
    {
        byte[] expectedBytes =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225,
            71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        ];
        var value = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        using var buffer = new WriteBuffer();

        PgLineSegment.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Fact]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsPgCircle()
    {
        var expectedValue = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        byte[] binaryData =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 19, 122, 225,
            71, 174, 20, 123, 64, 6, 102, 102, 102, 102, 102, 102,
        ];
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgLineSegment actualValue = PgLineSegment.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DecodeText_Should_DecodeTextEncodedValueAsPgCircle()
    {
        const string textData = "((5.63,8.59),(4.87,2.8))";
        var expectedValue = new PgLineSegment(new PgPoint(5.63, 8.59), new PgPoint(4.87, 2.8));
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgLineSegment actualValue = PgLineSegment.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("((1,1),(1,1),(1,1))")]
    [InlineData("((1,1))")]
    public void DecodeText_Should_Fail_When_InvalidText(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgLineSegment.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgLineSegment", e.Message);
            Assert.Contains("Line segments must have exactly 2 points", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnLineType() => Assert.Equal(PgLineSegment.DbType, PgTypeInfo.Lseg);

    [Fact]
    public void ArrayDbType_Should_ReturnLineArrayType() =>
        Assert.Equal(PgLineSegment.ArrayDbType, PgTypeInfo.LsegArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgLineSegment.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Lseg, true];
        yield return [PgTypeInfo.LsegArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
