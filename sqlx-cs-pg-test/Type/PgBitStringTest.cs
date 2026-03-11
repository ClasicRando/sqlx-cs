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
    [Test]
    [Arguments(
        new[] { true, false, true, false, true, false, true, false },
        new byte[] { 0, 0, 0, 8, 0b10101010 })]
    [Arguments(new[] { true, false, true, false }, new byte[] { 0, 0, 0, 4, 0b10100000 })]
    public async Task Encode_Should_WriteVarBit(bool[] bits, byte[] expectedBytes)
    {
        var value = new BitArray(bits);
        using var buffer = new PooledArrayBufferWriter();

        PgBitString.Encode(value, buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(
        new byte[] { 0, 0, 0, 8, 0b10101010 },
        new[] { true, false, true, false, true, false, true, false })]
    [Arguments(new byte[] { 0, 0, 0, 4, 0b10100000 }, new[] { true, false, true, false })]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsVarBit(
        byte[] binaryData,
        bool[] bits)
    {
        var expectedValue = new BitArray(bits);
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        BitArray actualValue = PgBitString.DecodeBytes(binaryValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    [Test]
    [Arguments("10101010", new[] { true, false, true, false, true, false, true, false })]
    [Arguments("1010", new[] { true, false, true, false })]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsVarBit(
        string textData,
        bool[] bits)
    {
        var expectedValue = new BitArray(bits);
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        BitArray actualValue = PgBitString.DecodeText(textValue);

        await Assert.That(actualValue).IsEquivalentTo(expectedValue);
    }

    [Test]
    [Arguments("012")]
    public async Task DecodeText_Should_Fail_When_AnyCharacterIsInvalid(string textData)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgBitString.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.Collections.BitArray");
            await Assert.That(e.Message).Contains("Could not decode char");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnVarBitType() =>
        await Assert.That(PgBitString.DbType).IsEqualTo(PgTypeInfo.Varbit);

    [Test]
    public async Task ArrayDbType_Should_ReturnVarBitArrayType() =>
        await Assert.That(PgBitString.ArrayDbType).IsEqualTo(PgTypeInfo.VarbitArray);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgBitString.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Varbit, true);
        yield return () => (PgTypeInfo.Bit, true);
        yield return () => (PgTypeInfo.VarbitArray, false);
        yield return () => (PgTypeInfo.Int4, false);
    }
}
