using System.Buffers;
using System.Buffers.Binary;
using JetBrains.Annotations;
using NSubstitute;

namespace Sqlx.Core.Buffer;

[TestSubject(typeof(BufferExtensions))]
public class BufferExtensionsTest
{
    public class ReadOnlySpan
    {
        [Test]
        [Arguments(54)]
        [Arguments(byte.MaxValue)]
        [Arguments(byte.MinValue)]
        public async Task ReadByte_Should_ReturnByte(byte value)
        {
            byte[] bytes = [value];
            ReadOnlySpan<byte> buffer = bytes;

            var actualValue = buffer.ReadByte();

            await Assert.That(actualValue).IsEqualTo(value);
        }

        [Test]
        public async Task ReadByte_Should_ThrowException_When_OutsideOfBounds()
        {
            byte[] bytes = [];
            ReadOnlySpan<byte> buffer = bytes;

            try
            {
                buffer.ReadByte();
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                await Assert.That(e).IsTypeOf<IndexOutOfRangeException>();
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
            ReadOnlySpan<byte> buffer = bytes;

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
            byte[] bytes =
            [
                (byte)(intValue & 0xff),
                (byte)(intValue >> 8 & 0xff),
                (byte)(intValue >> 16 & 0xff),
                (byte)(intValue >> 24 & 0xff),
            ];
            ReadOnlySpan<byte> buffer = bytes;

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
            byte[] bytes =
            [
                (byte)(longValue & 0xff),
                (byte)(longValue >> 8 & 0xff),
                (byte)(longValue >> 16 & 0xff),
                (byte)(longValue >> 24 & 0xff),
                (byte)(longValue >> 32 & 0xff),
                (byte)(longValue >> 40 & 0xff),
                (byte)(longValue >> 48 & 0xff),
                (byte)(longValue >> 56 & 0xff),
            ];
            ReadOnlySpan<byte> buffer = bytes;

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
            byte[] bytes =
            [
                (byte)(floatValue & 0xff),
                (byte)(floatValue >> 8 & 0xff),
                (byte)(floatValue >> 16 & 0xff),
                (byte)(floatValue >> 24 & 0xff),
            ];
            ReadOnlySpan<byte> buffer = bytes;

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
            byte[] bytes =
            [
                (byte)(doubleValue & 0xff),
                (byte)(doubleValue >> 8 & 0xff),
                (byte)(doubleValue >> 16 & 0xff),
                (byte)(doubleValue >> 24 & 0xff),
                (byte)(doubleValue >> 32 & 0xff),
                (byte)(doubleValue >> 40 & 0xff),
                (byte)(doubleValue >> 48 & 0xff),
                (byte)(doubleValue >> 56 & 0xff),
            ];
            ReadOnlySpan<byte> buffer = bytes;

            var actualValue = buffer.ReadDouble();

            await Assert.That(actualValue).IsEqualTo(value);
        }

        [Test]
        public async Task ReadBytesAsSpan_Should_ReturnReadOnlySpan()
        {
            byte[] bytes = [1, 2, 3, 255, 4];
            ReadOnlySpan<byte> buffer = bytes;

            var actualSpan = buffer.ReadBytesAsSpan(4).ToArray();

            await Assert.That(actualSpan).IsEquivalentTo(bytes[..4]);
        }

        [Test]
        public async Task ReadBytes_Should_ReturnByteArray()
        {
            byte[] bytes = [1, 2, 3, 255, 4];
            ReadOnlySpan<byte> buffer = bytes;

            var actualArray = buffer.ReadBytes(4);

            await Assert.That(actualArray).IsEquivalentTo(bytes[..4]);
        }

        [Test]
        public async Task ReadString_Should_ReturnString()
        {
            var bytes = "This is a test. Not in result"u8.ToArray();
            ReadOnlySpan<byte> buffer = bytes;

            var actualString = buffer.ReadString(14);

            await Assert.That(actualString).IsEqualTo("This is a test");
        }

        [Test]
        public async Task ReadCString_Should_ReturnStringUntilNullTerminator()
        {
            var bytes = "This is a test\0 Not in result\0"u8.ToArray();
            ReadOnlySpan<byte> buffer = bytes;

            var actualString = buffer.ReadCString();

            await Assert.That(actualString).IsEqualTo("This is a test");
        }
    }
}
