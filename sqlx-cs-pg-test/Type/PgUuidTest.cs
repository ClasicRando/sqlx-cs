using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBytea))]
public class PgUuidTest
{
    [Theory]
    [InlineData(
        "019a22a1-8d4c-7e71-8ac5-e31d330b866c",
        new byte[] { 1, 154, 34, 161, 141, 76, 126, 113, 138, 197, 227, 29, 51, 11, 134, 108 })]
    [InlineData(
        "019a22a1-c5bc-75c5-baf1-8199cfc9d061",
        new byte[] { 1, 154, 34, 161, 197, 188, 117, 197, 186, 241, 129, 153, 207, 201, 208, 97 })]
    public void Encode_Should_WriteGuid(string uuid, byte[] address)
    {
        Guid value = Guid.Parse(uuid);
        using var buffer = new WriteBuffer();

        PgUuid.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(address, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[] { 153, 34, 154, 1, 142, 184, 211, 115, 146, 27, 178, 250, 187, 200, 138, 60 },
        "019a2299-b88e-73d3-921b-b2fabbc88a3c")]
    [InlineData(
        new byte[] { 153, 34, 154, 1, 251, 251, 61, 121, 174, 159, 194, 153, 226, 118, 209, 34 },
        "019a2299-fbfb-793d-ae9f-c299e276d122")]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsGuid(byte[] binaryData, string uuid)
    {
        Guid expectedValue = Guid.Parse(uuid);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        Guid actualValue = PgUuid.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("019a2299-b88e-73d3-921b-b2fabbc88a3c")]
    [InlineData("019a2299-fbfb-793d-ae9f-c299e276d122")]
    public void DecodeText_Should_DecodeTextEncodedValueAsGuid(string textData)
    {
        Guid expectedValue = Guid.Parse(textData);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        Guid actualValue = PgUuid.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_FirstCharacterIsNotValid(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgUuid.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Guid", e.Message);
            Assert.Contains("Could not parse ", e.Message);
            Assert.Contains(" into a Guid", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnUuidType() => Assert.Equal(PgUuid.DbType, PgType.Uuid);

    [Fact]
    public void ArrayDbType_Should_ReturnUuidType() =>
        Assert.Equal(PgUuid.ArrayDbType, PgType.UuidArray);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgUuid.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Uuid, true];
        yield return [PgType.UuidArray, false];
        yield return [PgType.Int4, false];
    }

    [Fact]
    public void GetActualType()
    {
        Assert.Equal(PgType.Uuid, PgUuid.GetActualType(Guid.Empty));
    }
}
