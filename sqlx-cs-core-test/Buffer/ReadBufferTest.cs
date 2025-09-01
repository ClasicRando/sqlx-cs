using System.Buffers.Binary;
using JetBrains.Annotations;

namespace Sqlx.Core.Buffer;

[TestSubject(typeof(ReadBuffer))]
public class ReadBufferTest
{
    [Theory]
    [MemberData(nameof(SkipTestCases))]
    public void Skip_Should_ReturnSliceBounds(byte[] bytes, int skipNumber, Range expectedSlice)
    {
        var buffer = new ReadBuffer(bytes);

        Range range = buffer.Skip(skipNumber);
        
        Assert.Equal(expectedSlice, range);
    }

    public static IEnumerable<object[]> SkipTestCases()
    {
        yield return [new byte [] { 1, 2 }, 1, new Range(0, 1)];
        yield return [new byte [] { 1, 2, 3, 4, 5 }, 2, new Range(0, 2)];
    }
    
    [Theory]
    [InlineData(54)]
    [InlineData(byte.MaxValue)]
    [InlineData(byte.MinValue)]
    public void ReadByte_Should_ReturnByte(byte value)
    {
        byte[] bytes = [value];
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadByte();
        
        Assert.Equal(value, actualValue);
    }
    
    [Fact]
    public void ReadByte_Should_ThrowException_When_OutsideOfBounds()
    {
        byte[] bytes = [];
        var buffer = new ReadBuffer(bytes);

        try
        {
            buffer.ReadByte();
            Assert.Fail("Exception should have been thrown");
        }
        catch (Exception e)
        {
            #if DEBUG
                Assert.IsType<InvalidOperationException>(e);
            #else
                Assert.IsType<IndexOutOfRangeException>(e);
            #endif
        }
    }
    
    [Theory]
    [InlineData(589)]
    [InlineData(short.MaxValue)]
    [InlineData(short.MinValue)]
    public void ReadShort_Should_ReturnShort(short value)
    {
        var shortValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [(byte)(shortValue & 0xff), (byte)(shortValue >> 8)];
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadShort();
        
        Assert.Equal(value, actualValue);
    }
    
    [Theory]
    [InlineData(38023)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ReadInt_Should_ReturnInt(int value)
    {
        var intValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [
            (byte)(intValue & 0xff),
            (byte)(intValue >> 8 & 0xff),
            (byte)(intValue >> 16 & 0xff),
            (byte)(intValue >> 24 & 0xff),
        ];
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadInt();
        
        Assert.Equal(value, actualValue);
    }
    
    [Theory]
    [InlineData(2204379902L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void ReadLong_Should_ReturnLong(long value)
    {
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
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadLong();
        
        Assert.Equal(value, actualValue);
    }
    
    [Theory]
    [InlineData(52.365F)]
    [InlineData(float.MaxValue)]
    [InlineData(float.MinValue)]
    public void ReadFloat_Should_ReturnFloat(float value)
    {
        var floatValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value))
            : BitConverter.SingleToInt32Bits(value);
        byte[] bytes = [
            (byte)(floatValue & 0xff),
            (byte)(floatValue >> 8 & 0xff),
            (byte)(floatValue >> 16 & 0xff),
            (byte)(floatValue >> 24 & 0xff),
        ];
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadFloat();
        
        Assert.Equal(value, actualValue);
    }
    
    [Theory]
    [InlineData(3.4028234663852886E+38D)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    public void ReadDouble_Should_ReturnDouble(double value)
    {
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
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadDouble();
        
        Assert.Equal(value, actualValue);
    }

    [Fact]
    public void ReadBytesAsSpan_Should_ReturnReadOnlySpan()
    {
        byte[] bytes = [1, 2, 3, 255, 4];
        var buffer = new ReadBuffer(bytes);

        var actualSpan = buffer.ReadBytesAsSpan(4).ToArray();
        
        Assert.Equal(bytes[..4], actualSpan);
    }

    [Fact]
    public void ReadBytes_Should_ReturnByteArray()
    {
        byte[] bytes = [1, 2, 3, 255, 4];
        var buffer = new ReadBuffer(bytes);

        var actualArray = buffer.ReadBytes(4);
        
        Assert.Equal(bytes[..4], actualArray);
    }

    [Fact]
    public void ReadText_Should_ReturnString()
    {
        var bytes = "This is a test. Not in result"u8.ToArray();
        var buffer = new ReadBuffer(bytes);

        var actualString = buffer.ReadText(14);
        
        Assert.Equal("This is a test", actualString);
    }

    [Fact]
    public void ReadCString_Should_ReturnStringUntilNullTerminator()
    {
        var bytes = "This is a test\0 Not in result\0"u8.ToArray();
        var buffer = new ReadBuffer(bytes);

        var actualString = buffer.ReadCString();
        
        Assert.Equal("This is a test", actualString);
    }

    [Fact]
    public void Slice_Should_ReturnSubsetOfBufferAndAdvancePositionUntilAfterSlice()
    {
        byte[] bytes = [1, 2, 3, 4, 5];
        var buffer = new ReadBuffer(bytes);

        ReadBuffer slice = buffer.Slice(3);
        
        Assert.Equal(2, buffer.Remaining);
        Assert.Equal([1, 2, 3], slice.ReadBytes());
    }

    [Fact]
    public void Reset_Should_ReturnPositionToStart()
    {
        byte[] bytes = [1, 2, 3, 4, 5];
        var buffer = new ReadBuffer(bytes);

        buffer.Skip(2);
        Assert.Equal(3, buffer.Remaining);
        
        buffer.Reset();
        
        Assert.Equal(5, buffer.Remaining);
    }
}
