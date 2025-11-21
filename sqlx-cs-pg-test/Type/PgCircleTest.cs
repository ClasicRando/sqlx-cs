using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgCircle))]
public class PgCircleTest
{
    [Fact]
    public void Encode_Should_WritePgCircle()
    {
        byte[] expectedBytes =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 16, 0, 0, 0,
            0, 0, 0,
        ];
        var value = new PgCircle(new PgPoint(5.63, 8.59), 4);
        using var buffer = new WriteBuffer();

        PgCircle.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Fact]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsPgCircle()
    {
        var expectedValue = new PgCircle(new PgPoint(5.63, 8.59), 4);
        byte[] binaryData =
        [
            64, 22, 133, 30, 184, 81, 235, 133, 64, 33, 46, 20, 122, 225, 71, 174, 64, 16, 0, 0, 0,
            0, 0, 0,
        ];
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgCircle actualValue = PgCircle.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void DecodeText_Should_DecodeTextEncodedValueAsPgCircle()
    {
        const string textData = "<(5.63,8.59),4>";
        var expectedValue = new PgCircle(new PgPoint(5.63, 8.59), 4);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgCircle actualValue = PgCircle.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("(error,error),error", "Could not parse X coordinate")]
    [InlineData("<(1,2),error>", "Could not parse radius from ")]
    public void DecodeText_Should_Fail_When_InvalidText(
        string textData,
        string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgCircle.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgCircle", e.Message);
            Assert.Contains(contains, e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnCircleType() => Assert.Equal(PgCircle.DbType, PgTypeInfo.Circle);

    [Fact]
    public void ArrayDbType_Should_ReturnCircleArrayType() =>
        Assert.Equal(PgCircle.ArrayDbType, PgTypeInfo.CircleArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgCircle.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgTypeInfo.Circle, true];
        yield return [PgTypeInfo.CircleArray, false];
        yield return [PgTypeInfo.Int4, false];
    }
}
