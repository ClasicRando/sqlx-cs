using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBox))]
public class PgBoxTest
{
    [Theory]
    [InlineData(
        1,
        2,
        3,
        4,
        new byte[]
        {
            63, 240, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 64, 8, 0, 0, 0, 0, 0, 0, 64, 16, 0,
            0, 0, 0, 0, 0,
        })]
    [InlineData(
        4,
        3,
        2,
        1,
        new byte[]
        {
            64, 16, 0, 0, 0, 0, 0, 0, 64, 8, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 63, 240, 0,
            0, 0, 0, 0, 0,
        })]
    public void Encode_Should_WriteBox(
        double x1,
        double y1,
        double x2,
        double y2,
        byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();
        var value = new PgBox(new PgPoint(x1, y1), new PgPoint(x2, y2));

        PgBox.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[]
        {
            63, 240, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 64, 8, 0, 0, 0, 0, 0, 0, 64, 16, 0,
            0, 0, 0, 0, 0,
        },
        1,
        2,
        3,
        4)]
    [InlineData(
        new byte[]
        {
            64, 16, 0, 0, 0, 0, 0, 0, 64, 8, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 63, 240, 0,
            0, 0, 0, 0, 0,
        },
        4,
        3,
        2,
        1)]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsBox(
        byte[] binaryData,
        double x1,
        double y1,
        double x2,
        double y2)
    {
        var expectedValue = new PgBox(new PgPoint(x1, y1), new PgPoint(x2, y2));
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        PgBox actualValue = PgBox.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("(1,2),(3,4)", 1, 2, 3, 4)]
    [InlineData("(4,3),(2,1)", 4, 3, 2, 1)]
    public void DecodeText_Should_DecodeTextEncodedValueAsBox(
        string textData,
        double x1,
        double y1,
        double x2,
        double y2)
    {
        var expectedValue = new PgBox(new PgPoint(x1, y1), new PgPoint(x2, y2));
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        PgBox actualValue = PgBox.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("(1,2)")]
    [InlineData("(1,2),(3,4),(5,6)")]
    public void DecodeText_Should_Fail_When_LiteralDoesNotHave2Points(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgBox.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: Sqlx.Postgres.Type.PgBox", e.Message);
            Assert.Contains("Box geoms must have exactly 2 points", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnBoxType() => Assert.Equal(PgBox.DbType, PgType.Box);

    [Fact]
    public void ArrayDbType_Should_ReturnBoxType() =>
        Assert.Equal(PgBox.ArrayDbType, PgType.BoxArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgBox.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Box, true];
        yield return [PgType.BoxArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        var value = new PgBox(new PgPoint(1, 2), new PgPoint(3, 4));
        Assert.Equal(PgType.Box, PgBox.GetActualType(value));
    }
}
