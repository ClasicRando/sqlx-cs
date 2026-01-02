using System.Buffers;
using System.Buffers.Binary;

namespace Sqlx.Core.Buffer;

public static class BufferExtensions
{
    extension(ReadOnlySpan<byte> span)
    {
        public ReadOnlySpan<byte> ReadByte(out byte result)
        {
            if (span.Length < 1)
            {
                throw new ArgumentOutOfRangeException(
                    message: "Span must not be empty",
                    innerException: null);
            }
            result = span[0];
            return span[sizeof(byte)..];
        }
        
        public ReadOnlySpan<byte> ReadShort(out short result)
        {
            var temp = BitConverter.ToInt16(span[..sizeof(short)]);
            result = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(temp) : temp;
            return span[sizeof(short)..];
        }
        
        public ReadOnlySpan<byte> ReadInt(out int result)
        {
            var temp = BitConverter.ToInt32(span[..sizeof(int)]);
            result = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(temp) : temp;
            return span[sizeof(int)..];
        }
        
        public ReadOnlySpan<byte> ReadLong(out long result)
        {
            var temp = BitConverter.ToInt64(span[..sizeof(long)]);
            result = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(temp) : temp;
            return span[sizeof(long)..];
        }
    }

    extension(IBufferWriter<byte> bufferWriter)
    {
        public void WriteByte(byte value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(byte)];
            tempSpan[0] = value;
            bufferWriter.Write(tempSpan);
        }

        public void WriteShort(short value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(short)];
            BitConverter.TryWriteBytes(
                tempSpan,
                BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
            bufferWriter.Write(tempSpan);
        }

        public void WriteInt(int value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(int)];
            BitConverter.TryWriteBytes(
                tempSpan,
                BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
            bufferWriter.Write(tempSpan);
        }
        
        public void WriteUInt(uint value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(uint)];
            BitConverter.TryWriteBytes(
                tempSpan,
                BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
            bufferWriter.Write(tempSpan);
        }

        public void WriteLong(long value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(long)];
            BitConverter.TryWriteBytes(
                tempSpan,
                BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
            bufferWriter.Write(tempSpan);
        }

        public void WriteFloat(float value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(float)];
            if (BitConverter.IsLittleEndian)
            {
                BitConverter.TryWriteBytes(
                    tempSpan,
                    BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value)));
            }
            else
            {
                BitConverter.TryWriteBytes(tempSpan, value);
            }
            bufferWriter.Write(tempSpan);
        }

        public void WriteDouble(double value)
        {
            Span<byte> tempSpan = stackalloc byte[sizeof(double)];
            if (BitConverter.IsLittleEndian)
            {
                BitConverter.TryWriteBytes(
                    tempSpan,
                    BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value)));
            }
            else
            {
                BitConverter.TryWriteBytes(tempSpan, value);
            }
            bufferWriter.Write(tempSpan);
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            bufferWriter.Write(bytes);
        }

        public void WriteBytes(ReadOnlyMemory<byte> bytes)
        {
            bufferWriter.Write(bytes.Span);
        }

        public void WriteString(ReadOnlySpan<char> value)
        {
            const int maxStackAllocSize = 1024 / (sizeof(char) / sizeof(byte));
            var size = Charsets.Default.GetByteCount(value);
            var tempSpan = size > maxStackAllocSize ? new byte[size] : stackalloc byte[size];
            Charsets.Default.GetBytes(value, tempSpan);
            bufferWriter.Write(tempSpan);
        }

        /// <summary>
        /// Write the specified chars with a null termination to replicate a CString
        /// </summary>
        /// <param name="value">string to write</param>
        public void WriteCString(ReadOnlySpan<char> value)
        {
            if (value.Length != 0)
            {
                bufferWriter.WriteString(value);
            }
            bufferWriter.WriteByte(0);
        }
    }
    
    extension(ReadOnlySequence<byte> sequence)
    {
        public ReadOnlySequence<byte> ReadShort(out short result)
        {
            short temp;
            if (sequence.IsSingleSegment)
            {
                var span = sequence.FirstSpan;
                temp = BitConverter.ToInt16(span[..sizeof(short)]);
            }
            else
            {
                Span<byte> tempSpan = stackalloc byte[sizeof(short)];
                sequence.CopyTo(tempSpan);
                temp = BitConverter.ToInt16(tempSpan[..sizeof(short)]);
            }
            
            result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            return sequence.Slice(sizeof(short));
        }
        
        public ReadOnlySequence<byte> ReadInt(out int result)
        {
            int temp;
            if (sequence.IsSingleSegment)
            {
                var span = sequence.FirstSpan;
                temp = BitConverter.ToInt32(span[..sizeof(int)]);
            }
            else
            {
                Span<byte> tempSpan = stackalloc byte[sizeof(int)];
                sequence.CopyTo(tempSpan);
                temp = BitConverter.ToInt32(tempSpan[..sizeof(int)]);
            }
            
            result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            return sequence.Slice(sizeof(int));
        }
    }
}
