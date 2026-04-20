using JetBrains.Annotations;
using Sqlx.Core.Buffer;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

[TestSubject(typeof(PgOid))]
public class PgOidTest
{
    [Test]
    [Arguments(0, new byte[] { 0, 0, 0, 0 })]
    [Arguments(short.MaxValue, new byte[] { 0, 0, 127, 255 })]
    [Arguments(int.MaxValue, new byte[] { 127, 255, 255, 255 })]
    [Arguments(uint.MaxValue, new byte[] { 255, 255, 255, 255 })]
    public async Task Encode_Should_WriteInt(uint value, byte[] expectedBytes)
    {
        using var buffer = new ArrayBufferWriter();

        PgOid.Encode(new PgOid(value), buffer);

        var actualBytes = buffer.ReadableSpan.ToArray();

        await Assert.That(actualBytes).IsEquivalentTo(expectedBytes);
    }

    [Test]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0)]
    [Arguments(new byte[] { 127, 255 }, short.MaxValue)]
    [Arguments(new byte[] { 0, 0, 127, 255 }, short.MaxValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 127, 255 }, short.MaxValue)]
    [Arguments(new byte[] { 127, 255, 255, 255 }, int.MaxValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 127, 255, 255, 255 }, int.MaxValue)]
    [Arguments(new byte[] { 255, 255, 255, 255 }, uint.MaxValue)]
    [Arguments(new byte[] { 0, 0, 0, 0, 255, 255, 255, 255 }, uint.MaxValue)]
    public async Task DecodeBytes_Should_DecodeBinaryEncodedValueAsInt(
        byte[] binaryData,
        uint expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);

        PgOid actualValue = PgOid.DecodeBytes(binaryValue);

        await Assert.That(actualValue.Inner).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments(new byte[] { 255, 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 127, 255, 255, 255, 255, 255, 255, 255 })]
    public async Task DecodeBytes_Should_Fail_When_OutsideOfIntBounds(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);
        try
        {
            PgOid temp = PgOid.DecodeBytes(binaryValue);
            Assert.Fail($"Decoding should have failed. Found '{temp}'");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.UInt32");
            await Assert.That(e.Message).Contains("Value is outside of valid UInt32");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments(new byte[] { 0 })]
    [Arguments(new byte[] { 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0 })]
    [Arguments(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public async Task DecodeBytes_Should_Fail_When_InvalidNumberOfBytes(byte[] binaryData)
    {
        var columnMetadata = new PgColumnMetadata();
        var binaryValue = new PgBinaryValue(binaryData, in columnMetadata);
        try
        {
            PgOid.DecodeBytes(binaryValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.UInt32");
            await Assert.That(e.Message).Contains("Could not extract integer from buffer. Number of bytes = ");
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    [Arguments("0", 0)]
    [Arguments("32767", short.MaxValue)]
    [Arguments("2147483647", int.MaxValue)]
    [Arguments("4294967295", uint.MaxValue)]
    public async Task DecodeText_Should_DecodeTextEncodedValueAsInt(
        string textData,
        uint expectedValue)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        PgOid actualValue = PgOid.DecodeText(textValue);

        await Assert.That(actualValue.Inner).IsEqualTo(expectedValue);
    }

    [Test]
    [Arguments("error", "Could not convert 'error' into System.UInt32")]
    [Arguments("-9223372036854775808", "Value is outside of valid UInt32")]
    [Arguments("9223372036854775807", "Value is outside of valid UInt32")]
    public async Task DecodeText_Should_Fail_When_InvalidIntString(string textData, string contains)
    {
        var columnMetadata = new PgColumnMetadata();
        var textValue = new PgTextValue(textData, in columnMetadata);

        try
        {
            PgOid.DecodeText(textValue);
            Assert.Fail("Decoding should have failed");
        }
        catch (ColumnDecodeException e)
        {
            await Assert.That(e.Message).Contains("Desired Output: System.UInt32");
            await Assert.That(e.Message).Contains(contains);
        }
        catch (Exception e)
        {
            Assert.Fail($"Decoding should have failed due to column decode error. Instead: {e}");
        }
    }

    [Test]
    public async Task DbType_Should_ReturnIntType() => await Assert.That(PgOid.DbType).IsEqualTo(PgTypeInfo.Oid);

    [Test]
    public async Task ArrayDbType_Should_ReturnIntType() =>
        await Assert.That(PgOid.ArrayDbType).IsEqualTo(PgTypeInfo.OidArray);

    [Test]
    [MethodDataSource(nameof(IsCompatibleCases))]
    public async Task IsCompatible(PgTypeInfo pgType, bool expectedResult) =>
        await Assert.That(PgInt.IsCompatible(pgType)).IsEqualTo(expectedResult);

    public static IEnumerable<Func<(PgTypeInfo, bool)>> IsCompatibleCases()
    {
        yield return () => (PgTypeInfo.Int8, true);
        yield return () => (PgTypeInfo.OidArray, false);
        yield return () => (PgTypeInfo.Oid, true);
        yield return () => (PgTypeInfo.Int4, true);
        yield return () => (PgTypeInfo.Int2, true);
    }
}
