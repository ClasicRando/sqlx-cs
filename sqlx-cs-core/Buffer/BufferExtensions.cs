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
        public void WriteByte(byte value, bool advance = true)
        {
            var span = bufferWriter.GetSpan();
            span[0] = value;
            if (advance) bufferWriter.Advance(sizeof(byte));
        }

        public void WriteShort(short value, bool advance = true)
        {
            var span = bufferWriter.GetSpan(sizeof(short));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }
            BitConverter.TryWriteBytes(span, value);
            if (advance) bufferWriter.Advance(sizeof(short));
        }

        public void WriteInt(int value, bool advance = true)
        {
            var span = bufferWriter.GetSpan(sizeof(int));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }
            BitConverter.TryWriteBytes(span, value);
            if (advance) bufferWriter.Advance(sizeof(int));
        }
        
        public void WriteUInt(uint value, bool advance = true)
        {
            var span = bufferWriter.GetSpan(sizeof(uint));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }
            BitConverter.TryWriteBytes(span, value);
            if (advance) bufferWriter.Advance(sizeof(uint));
        }

        public void WriteLong(long value, bool advance = true)
        {
            var span = bufferWriter.GetSpan(sizeof(long));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }
            BitConverter.TryWriteBytes(span, value);
            if (advance) bufferWriter.Advance(sizeof(long));
        }

        public void WriteFloat(float value, bool advance = true)
        {
            var span = bufferWriter.GetSpan(sizeof(float));
            if (BitConverter.IsLittleEndian)
            {
                BitConverter.TryWriteBytes(
                    span,
                    BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value)));
            }
            else
            {
                BitConverter.TryWriteBytes(span, value);
            }
            if (advance) bufferWriter.Advance(sizeof(float));
        }

        public void WriteDouble(double value, bool advance = true)
        {
            var span = bufferWriter.GetSpan(sizeof(double));
            if (BitConverter.IsLittleEndian)
            {
                BitConverter.TryWriteBytes(
                    span,
                    BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value)));
            }
            else
            {
                BitConverter.TryWriteBytes(span, value);
            }
            if (advance) bufferWriter.Advance(sizeof(double));
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes, bool advance = true)
        {
            var span = bufferWriter.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            if (advance) bufferWriter.Advance(bytes.Length);
        }

        public void WriteBytes(ReadOnlyMemory<byte> bytes, bool advance = true)
        {
            bufferWriter.WriteBytes(bytes.Span, advance);
        }

        public void WriteString(ReadOnlySpan<char> value, bool advance = true)
        {
            var size = Charsets.Default.GetByteCount(value);
            var span = bufferWriter.GetSpan(size);
            Charsets.Default.GetBytes(value, span);
            if (advance) bufferWriter.Advance(size);
        }

        /// <summary>
        /// Write the specified chars with a null termination to replicate a CString
        /// </summary>
        /// <param name="value">String to write</param>
        /// <param name="advance">True if </param>
        public void WriteCString(ReadOnlySpan<char> value, bool advance = true)
        {
            if (value.Length != 0)
            {
                bufferWriter.WriteString(value, advance);
            }
            bufferWriter.WriteByte(0, advance);
        }

        /// <summary>
        /// Perform the supplied write action, eventually performing the same action against this
        /// buffer writer. This method should only be used when the write action is completely
        /// unknown to the caller or the sizing of such an action is impossible or costly.
        /// Internally this method allocates a <see cref="WriteBuffer"/> to be a proxy for the write
        /// action after which the number of bytes written during the action (as an
        /// <see cref="int"/>) is written to this buffer followed by the entire write action
        /// contents.
        /// </summary>
        /// <param name="writeAction">Write actions that must be length prefixed</param>
        /// <param name="includeLength">
        /// True to include the number of bytes used to store the length in the length calculation,
        /// otherwise, just bytes written during the action are included
        /// </param>
        public void WriteLengthPrefixed(
            Action<IBufferWriter<byte>> writeAction,
            bool includeLength = true)
        {
            using WriteBuffer tempWriter = new(initialCapacity: 1024);
            writeAction(tempWriter);
            var length = tempWriter.WrittenCount + (includeLength ? sizeof(int) : 0);
            var totalWritten = sizeof(int) + length;
            var span = bufferWriter.GetSpan(totalWritten);
            BitConverter.TryWriteBytes(
                span,
                BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(length) : length);
            tempWriter.ReadableSpan.CopyTo(span[4..]);
            bufferWriter.Advance(totalWritten);
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
