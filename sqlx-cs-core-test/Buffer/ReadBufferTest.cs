using System.Buffers.Binary;
using JetBrains.Annotations;

namespace Sqlx.Core.Buffer;

[TestSubject(typeof(ReadBuffer))]
public class ReadBufferTest
{
    [Test]
    [MethodDataSource(nameof(SkipTestCases))]
    public async Task Skip_Should_ReturnSliceBounds(byte[] bytes, int skipNumber, Range expectedSlice)
    {
        var buffer = new ReadBuffer(bytes);

        Range range = buffer.Skip(skipNumber);
        
        await Assert.That(range).IsEqualTo(expectedSlice);
    }

    public static IEnumerable<object[]> SkipTestCases()
    {
        yield return [new byte [] { 1, 2 }, 1, new Range(0, 1)];
        yield return [new byte [] { 1, 2, 3, 4, 5 }, 2, new Range(0, 2)];
    }
    
    [Test]
    [Arguments(54)]
    [Arguments(byte.MaxValue)]
    [Arguments(byte.MinValue)]
    public async Task ReadByte_Should_ReturnByte(byte value)
    {
        byte[] bytes = [value];
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadByte();
        
        await Assert.That(actualValue).IsEqualTo(value);
    }
    
    [Test]
    public async Task ReadByte_Should_ThrowException_When_OutsideOfBounds()
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
                await Assert.That(e).IsTypeOf<InvalidOperationException>();
            #else
                await Assert.That(e).IsTypeOf<IndexOutOfRangeException>();
            #endif
        }
    }
    
    [Test]
    [Arguments(589)]
    [Arguments(short.MaxValue)]
    [Arguments(short.MinValue)]
    public async Task ReadShort_Should_ReturnShort(short value)
    {
        var shortValue = BitConverter.IsLittleEndian
            ? BinaryPrimitives.ReverseEndianness(value)
            : value;
        byte[] bytes = [(byte)(shortValue & 0xff), (byte)(shortValue >> 8)];
        var buffer = new ReadBuffer(bytes);

        var actualValue = buffer.ReadShort();
        
        await Assert.That(actualValue).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(38023)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    public async Task ReadInt_Should_ReturnInt(int value)
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
        
        await Assert.That(actualValue).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(2204379902L)]
    [Arguments(long.MaxValue)]
    [Arguments(long.MinValue)]
    public async Task ReadLong_Should_ReturnLong(long value)
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
        
        await Assert.That(actualValue).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(52.365F)]
    [Arguments(float.MaxValue)]
    [Arguments(float.MinValue)]
    public async Task ReadFloat_Should_ReturnFloat(float value)
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
        
        await Assert.That(actualValue).IsEqualTo(value);
    }
    
    [Test]
    [Arguments(3.4028234663852886E+38D)]
    [Arguments(double.MaxValue)]
    [Arguments(double.MinValue)]
    public async Task ReadDouble_Should_ReturnDouble(double value)
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
        
        await Assert.That(actualValue).IsEqualTo(value);
    }

    [Test]
    public async Task ReadBytesAsSpan_Should_ReturnReadOnlySpan()
    {
        byte[] bytes = [1, 2, 3, 255, 4];
        var buffer = new ReadBuffer(bytes);

        var actualSpan = buffer.ReadBytesAsSpan(4).ToArray();
        
        await Assert.That(actualSpan).IsEquivalentTo(bytes[..4]);
    }

    [Test]
    public async Task ReadBytes_Should_ReturnByteArray()
    {
        byte[] bytes = [1, 2, 3, 255, 4];
        var buffer = new ReadBuffer(bytes);

        var actualArray = buffer.ReadBytes(4);
        
        await Assert.That(actualArray).IsEquivalentTo(bytes[..4]);
    }

    [Test]
    public async Task ReadText_Should_ReturnString()
    {
        var bytes = "This is a test. Not in result"u8.ToArray();
        var buffer = new ReadBuffer(bytes);

        var actualString = buffer.ReadText(14);
        
        await Assert.That(actualString).IsEqualTo("This is a test");
    }

    [Test]
    public async Task ReadCString_Should_ReturnStringUntilNullTerminator()
    {
        var bytes = "This is a test\0 Not in result\0"u8.ToArray();
        var buffer = new ReadBuffer(bytes);

        var actualString = buffer.ReadCString();
        
        await Assert.That(actualString).IsEqualTo("This is a test");
    }

    [Test]
    public async Task Slice_Should_ReturnSubsetOfBufferAndAdvancePositionUntilAfterSlice()
    {
        byte[] bytes = [1, 2, 3, 4, 5];
        var buffer = new ReadBuffer(bytes);

        ReadBuffer slice = buffer.Slice(3);
        var remaining = buffer.Remaining;
        var actualBytes = slice.ReadBytes();
        
        await Assert.That(remaining).IsEqualTo(2);
        await Assert.That(actualBytes).IsEquivalentTo(new byte[] { 1, 2, 3 });
    }

    [Test]
    public async Task Reset_Should_ReturnPositionToStart()
    {
        byte[] bytes = [1, 2, 3, 4, 5];
        var buffer = new ReadBuffer(bytes);

        buffer.Skip(2);

        var remaining1 = buffer.Remaining;
        buffer.Reset();
        var remaining2 = buffer.Remaining;
        
        await Assert.That(remaining1).IsEqualTo(3);
        await Assert.That(remaining2).IsEqualTo(5);
    }
}