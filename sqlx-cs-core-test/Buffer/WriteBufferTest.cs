using System.Buffers.Binary;
using System.Text;
using JetBrains.Annotations;

namespace Sqlx.Core.Buffer;

[TestSubject(typeof(WriteBuffer))]
public class WriteBufferTest
{
    [Theory]
    [InlineData(54)]
    [InlineData(byte.MaxValue)]
    [InlineData(byte.MinValue)]
    public void WriteByte_Should_FillBufferWithByte(byte value)
    {
        using var buffer = new WriteBuffer();
        
        buffer.WriteByte(value);
        
        Assert.Equal([value], buffer.ReadableSpan.ToArray());
    }

    [Theory]
    [InlineData(589)]
    [InlineData(short.MaxValue)]
    [InlineData(short.MinValue)]
    public void WriteShort_Should_FillBufferWithShort(short value)
    {
        using var buffer = new WriteBuffer();
        var shortValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [(byte)(shortValue & 0xff), (byte)(shortValue >> 8)];
        
        buffer.WriteShort(value);
        
        Assert.Equal(bytes, buffer.ReadableSpan.ToArray());
    }

    [Theory]
    [InlineData(38023)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void WriteInt_Should_FillBufferWithInt(int value)
    {
        using var buffer = new WriteBuffer();
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
        
        Assert.Equal(bytes, buffer.ReadableSpan.ToArray());
    }

    [Theory]
    [InlineData(2204379902L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void WriteLong_Should_FillBufferWithLong(long value)
    {
        using var buffer = new WriteBuffer();
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
        
        Assert.Equal(bytes, buffer.ReadableSpan.ToArray());
    }

    [Theory]
    [InlineData(52.365F)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    public void WriteFloat_Should_FillBufferWithFloat(float value)
    {
        using var buffer = new WriteBuffer();
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
        
        Assert.Equal(bytes, buffer.ReadableSpan.ToArray());
    }

    [Theory]
    [InlineData(3.4028234663852886E+38D)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void WriteDouble_Should_FillBufferWithDouble(double value)
    {
        using var buffer = new WriteBuffer();
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
        
        Assert.Equal(bytes, buffer.ReadableSpan.ToArray());
    }

    [Fact]
    public void WriteBytes_Should_FillBufferWithBytes_When_Span()
    {
        using var buffer = new WriteBuffer();
        byte[] bytes = [1, 2, 3, 255, 4];

        buffer.WriteBytes(bytes.AsSpan(0, 4));
        
        Assert.Equal(bytes[..4], buffer.ReadableSpan.ToArray());
    }

    [Fact]
    public void WriteBytes_Should_FillBufferWithBytes_When_Memory()
    {
        using var buffer = new WriteBuffer();
        byte[] bytes = [1, 2, 3, 255, 4];

        buffer.WriteBytes(bytes.AsMemory(0, 4));
        
        Assert.Equal(bytes[..4], buffer.ReadableSpan.ToArray());
    }

    [Fact]
    public void WriteString_Should_FillBufferWithUtf8Bytes()
    {
        using var buffer = new WriteBuffer();
        const string value = "This is a test";

        buffer.WriteString(value);
        
        Assert.Equal(value, Encoding.UTF8.GetString(buffer.ReadableSpan));
    }

    [Fact]
    public void WriteCString_Should_FillBufferWithNullTerminatedUtf8Bytes()
    {
        using var buffer = new WriteBuffer();
        const string value = "This is a test";

        buffer.WriteCString(value);
        
        Assert.Equal(value, Encoding.UTF8.GetString(buffer.ReadableSpan[..^1]));
        Assert.Equal(0, buffer.ReadableSpan[^1]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteLengthPrefixed_Should_AllowForWritingToBufferWithALengthPrefix(bool includeLength)
    {
        using var buffer = new WriteBuffer();
        
        var startingPosition = buffer.StartWritingLengthPrefixed();
        buffer.WriteByte(1);
        buffer.FinishWritingLengthPrefixed(startingPosition, includeLength);

        var bytes = buffer.ReadableSpan.ToArray();
        var readBuffer = new ReadBuffer(bytes.AsSpan());
        Assert.Equal(1 + (includeLength ? 4 : 0), readBuffer.ReadInt());
        Assert.Equal(1, readBuffer.ReadByte());
    }
}
