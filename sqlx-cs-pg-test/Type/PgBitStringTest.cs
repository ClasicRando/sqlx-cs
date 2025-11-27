using System.Collections;
using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgBitString))]
public class PgBitStringTest
{
    [Theory]
    [InlineData(
        new[] { true, false, true, false, true, false, true, false },
        new byte[] { 0, 0, 0, 8, 0b10101010 })]
    [InlineData(new[] { true, false, true, false }, new byte[] { 0, 0, 0, 4, 0b10100000 })]
    public void Encode_Should_WriteVarBit(bool[] bits, byte[] expectedBytes)
    {
        var value = new BitArray(bits);
        using var buffer = new WriteBuffer();

        PgBitString.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        Assert.Equal(expectedBytes, actualBytes);
    }

    [Theory]
    [InlineData(
        new byte[] { 0, 0, 0, 8, 0b10101010 },
        new[] { true, false, true, false, true, false, true, false })]
    [InlineData(new byte[] { 0, 0, 0, 4, 0b10100000 }, new[] { true, false, true, false })]
    public void DecodeBytes_Should_DecodeBinaryEncodedValueAsVarBit(
        byte[] binaryData,
        bool[] bits)
    {
        var expectedValue = new BitArray(bits);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(new ReadBuffer(binaryData), ref columnMetadata);

        BitArray actualValue = PgBitString.DecodeBytes(ref binaryValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("10101010", new[] { true, false, true, false, true, false, true, false })]
    [InlineData("1010", new[] { true, false, true, false })]
    public void DecodeText_Should_DecodeTextEncodedValueAsVarBit(
        string textData,
        bool[] bits)
    {
        var expectedValue = new BitArray(bits);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        BitArray actualValue = PgBitString.DecodeText(textValue);

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("012")]
    public void DecodeText_Should_Fail_When_AnyCharacterIsInvalid(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, ref columnMetadata);

        try
        {
            PgBitString.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            Assert.Contains("Desired Output: System.Collections.BitArray", e.Message);
            Assert.Contains("Could not decode char", e.Message);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Fact]
    public void DbType_Should_ReturnVarBitType() =>
        Assert.Equal(PgTypeInfo.Varbit, PgBitString.DbType);

    [Fact]
    public void ArrayDbType_Should_ReturnVarBitArrayType() =>
        Assert.Equal(PgTypeInfo.VarbitArray, PgBitString.ArrayDbType);

    [Theory]
    [MemberData(nameof(IsCompatibleCases))]
    public void IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        Assert.Equal(expectedResult, PgBitString.IsCompatible(pgType));

    public static IEnumerable<TheoryDataRow<PgTypeInfo, bool>> IsCompatibleCases()
    {
        return new TheoryData<PgTypeInfo, bool>(
            (PgTypeInfo.Varbit, true),
            (PgTypeInfo.Bit, true),
            (PgTypeInfo.VarbitArray, false),
            (PgTypeInfo.Int4, false));
    }
}
