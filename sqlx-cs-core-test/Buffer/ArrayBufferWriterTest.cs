using System.Buffers.Binary;
using System.Text;
using JetBrains.Annotations;

namespace Sqlx.Core.Buffer;

[TestSubject(typeof(ArrayBufferWriter))]
public class ArrayBufferWriterTest
{
    [Test]
    [Arguments(54)]
    [Arguments(byte.MaxValue)]
    [Arguments(byte.MinValue)]
    public async Task WriteByte_Should_FillBufferWithByte(byte value)
    {
        using var buffer = new ArrayBufferWriter();
        
        buffer.WriteByte(value);
        
        await Assert.That(buffer.ReadableSpan.ToArray()).IsEquivalentTo([value]);
    }

    [Test]
    [Arguments(589)]
    [Arguments(short.MaxValue)]
    [Arguments(short.MinValue)]
    public async Task WriteShort_Should_FillBufferWithShort(short value)
    {
        using var buffer = new ArrayBufferWriter();
        var shortValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [(byte)(shortValue & 0xff), (byte)(shortValue >> 8)];
        
        buffer.WriteShort(value);
        
        await Assert.That(buffer.ReadableSpan.ToArray()).IsEquivalentTo(bytes);
    }

    [Test]
    [Arguments(38023)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    public async Task WriteInt_Should_FillBufferWithInt(int value)
    {
        using var buffer = new ArrayBufferWriter();
        var intValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [
            (byte)(intValue & 0xff),
            (byte)(intValue >> 8 & 0xff),
            (byte)(intValue >> 16 & 0xff),
            (byte)(intValue >> 24 & 0xff),
        ];
        
        buffer.WriteInt(value);
        
        await Assert.That(buffer.ReadableSpan.ToArray()).IsEquivalentTo(bytes);
    }

    [Test]
    [Arguments(2204379902L)]
    [Arguments(long.MaxValue)]
    [Arguments(long.MinValue)]
    public async Task WriteLong_Should_FillBufferWithLong(long value)
    {
        using var buffer = new ArrayBufferWriter();
        var longValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [
            (byte)(longValue & 0xff),
            (byte)(longValue >> 8 & 0xff),
            (byte)(longValue >> 16 & 0xff),
            (byte)(longValue >> 24 & 0xff),
            (byte)(longValue >> 32 & 0xff),
            (byte)(longValue >> 40 & 0xff),
            (byte)(longValue >> 48 & 0xff),
            (byte)(longValue >> 56 & 0xff),
        ];
        
        buffer.WriteLong(value);
        
        await Assert.That(buffer.ReadableSpan.ToArray()).IsEquivalentTo(bytes);
    }

    [Test]
    [Arguments(52.365F)]
    [Arguments(float.MaxValue)]
    [Arguments(float.MinValue)]
    public async Task WriteFloat_Should_FillBufferWithFloat(float value)
    {
        using var buffer = new ArrayBufferWriter();
        var floatValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value))
            : BitConverter.SingleToInt32Bits(value);
        byte[] bytes = [
            (byte)(floatValue & 0xff),
            (byte)(floatValue >> 8 & 0xff),
            (byte)(floatValue >> 16 & 0xff),
            (byte)(floatValue >> 24 & 0xff),
        ];
        
        buffer.WriteFloat(value);
        
        await Assert.That(buffer.ReadableSpan.ToArray()).IsEquivalentTo(bytes);
    }

    [Test]
    [Arguments(3.4028234663852886E+38D)]
    [Arguments(double.MaxValue)]
    [Arguments(double.MinValue)]
    public async Task WriteDouble_Should_FillBufferWithDouble(double value)
    {
        using var buffer = new ArrayBufferWriter();
        var doubleValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value))
            : BitConverter.DoubleToInt64Bits(value);
        byte[] bytes = [
            (byte)(doubleValue & 0xff),
            (byte)(doubleValue >> 8 & 0xff),
            (byte)(doubleValue >> 16 & 0xff),
            (byte)(doubleValue >> 24 & 0xff),
            (byte)(doubleValue >> 32 & 0xff),
            (byte)(doubleValue >> 40 & 0xff),
            (byte)(doubleValue >> 48 & 0xff),
            (byte)(doubleValue >> 56 & 0xff),
        ];
        
        buffer.WriteDouble(value);
        
        await Assert.That(buffer.ReadableSpan.ToArray()).IsEquivalentTo(bytes);
    }

    [Test]
    public async Task WriteString_Should_FillBufferWithUtf8Bytes()
    {
        using var buffer = new ArrayBufferWriter();
        const string value = "This is a test";

        buffer.WriteString(value);
        
        await Assert.That(Encoding.UTF8.GetString(buffer.ReadableSpan)).IsEqualTo(value);
    }

    [Test]
    public async Task WriteCString_Should_FillBufferWithNullTerminatedUtf8Bytes()
    {
        using var buffer = new ArrayBufferWriter();
        const string value = "This is a test";

        buffer.WriteCString(value);
        
        await Assert.That(Encoding.UTF8.GetString(buffer.ReadableSpan[..^1])).IsEqualTo(value);
        await Assert.That(buffer.ReadableSpan[^1]).IsEqualTo((byte)0);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task WriteLengthPrefixed_Should_AllowForWritingToBufferWithALengthPrefix(bool includeLength)
    {
        using var buffer = new ArrayBufferWriter();
        
        var startingPosition = buffer.StartWritingLengthPrefixed();
        buffer.WriteByte(1);
        buffer.FinishWritingLengthPrefixed(startingPosition, includeLength);

        var bytes = buffer.ReadableSpan.ToArray();
        ReadOnlySpan<byte> readBuffer = bytes.AsSpan();

        var firstInt = readBuffer.ReadInt();
        var firstByte = readBuffer.ReadByte();
        
        await Assert.That(firstInt).IsEqualTo(1 + (includeLength ? 4 : 0));
        await Assert.That(firstByte).IsEqualTo((byte)1);
    }
}