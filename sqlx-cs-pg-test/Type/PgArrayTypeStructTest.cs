using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgArrayTypeStruct<,>))]
public class PgArrayTypeStructTest
{
    [Theory]
    [MemberData(nameof(EncodeCases))]
    public void Encode_Should_WriteIntArray(int?[] value, byte[] expectedBytes)
    {
        using var buffer = new WriteBuffer();

        PgArrayTypeStruct<int, PgInt>.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }
    
    public static IEnumerable<object[]> EncodeCases()
    {
        yield return [Array.Empty<int?>(), new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 0, 0, 0, 0, 1 }];
        yield return [new int?[] { 1 }, new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 1 }];
        yield return [new int?[] { null, 1 }, new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0, 0, 4, 0, 0, 0, 1 }];
    }

    [Theory]
    [MemberData(nameof(DecodeBytesCases))]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsIntArray(byte[] binaryData, int?[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        var actualValue = PgArrayTypeStruct<int, PgInt>.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }
    
    public static IEnumerable<object[]> DecodeBytesCases()
    {
        yield return [new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 0, 0, 0, 0, 1 }, Array.Empty<int?>()];
        yield return [new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 1 }, new int?[] { 1 }];
        yield return [new byte[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 23, 0, 0, 0, 2, 0, 0, 0, 1, 255, 255, 255, 255, 0, 0, 0, 4, 0, 0, 0, 1 }, new int?[] { null, 1 }];
    }

    [Theory]
    [MemberData(nameof(DecodeTextCases))]
    public void DecodeText_Should_DecodeTextEncodedValueAsIntArray(string textData, int?[] expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        var actualValue = PgArrayTypeStruct<int, PgInt>.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }
    
    public static IEnumerable<object[]> DecodeTextCases()
    {
        yield return ["{}", Array.Empty<int?>()];
        yield return ["{1}", new int?[] { 1 }];
        yield return ["{NULL,1}", new int?[] { null, 1 }];
    }

    [Theory]
    [InlineData("error")]
    public void DecodeText_Should_Fail_When_InvalidArrayLiteral(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgArrayTypeStruct<int, PgInt>.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Int32[]", e.Message);
            Assert.Contains("Array literal must be enclosed in curly braces", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnArrayType() => Assert.Equal(
        PgArrayTypeStruct<int, PgInt>.DbType,
        PgType.Int4Array);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgType pgType, bool expectedResult) => Assert.Equal(
        expectedResult,
        PgArrayTypeStruct<int, PgInt>.IsCompatible(pgType));

    public static IEnumerable<object[]> IsCompatibleCases()
    {
        yield return [PgType.Int4Array, true];
        yield return [PgType.Int4, false];
        yield return [PgType.TextArray, false];
    }

    [Theory]
    [MemberData(nameof(GetActualTypeCases))]
    public void GetActualType(int?[] value, PgType expectedResult) => Assert.Equal(
        expectedResult,
        PgArrayTypeStruct<int, PgInt>.GetActualType(value));

    public static IEnumerable<object[]> GetActualTypeCases()
    {
        yield return [Array.Empty<int?>(), PgType.Int4Array];
        yield return [new int?[] { 1 }, PgType.Int4Array];
    }
}
