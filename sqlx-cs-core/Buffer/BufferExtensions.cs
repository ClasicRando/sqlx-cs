using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Sqlx.Core.Buffer;

public static class BufferExtensions
{
    extension(ref ReadOnlySpan<byte> span)
    {
        /// <summary>
        /// Modify this <see cref="ReadOnlySpan{byte}"/> reference to skip the desired number of
        /// bytes
        /// </summary>
        /// <param name="count">Number of bytes to skip</param>
        public void Skip(int count)
        {
            span = span[count..];
        }

        /// <summary>
        /// Consume the first byte of this span, returning that byte. The span will now point to all
        /// bytes after the first byte.
        /// </summary>
        /// <returns>First byte of this span</returns>
        public byte ReadByte()
        {
            if (span.IsEmpty)
            {
                throw new InvalidOperationException("Span must not be empty");
            }

            var result = span[0];
            span = span[sizeof(byte)..];
            return result;
        }

        /// <summary>
        /// Consume the first 2 bytes of this span, returning them as a <see cref="short"/>. The
        /// span will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 2 bytes of this span as a <see cref="short"/></returns>
        public short ReadShort()
        {
            var tempSpan = span.ReadBytesAsSpan(sizeof(short));
            var temp = BitConverter.ToInt16(tempSpan);
            return BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
        }

        /// <summary>
        /// Consume the first 4 bytes of this span, returning them as a <see cref="int"/>. The span
        /// will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 4 bytes of this span as a <see cref="int"/></returns>
        public int ReadInt()
        {
            var tempSpan = span.ReadBytesAsSpan(sizeof(int));
            var temp = BitConverter.ToInt32(tempSpan);
            return BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
        }

        /// <summary>
        /// Consume the first 4 bytes of this span, returning them as a <see cref="uint"/>. The span
        /// will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 4 bytes of this span as a <see cref="uint"/></returns>
        public uint ReadUInt()
        {
            var tempSpan = span.ReadBytesAsSpan(sizeof(uint));
            var temp = BitConverter.ToUInt32(tempSpan);
            return BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
        }

        /// <summary>
        /// Consume the first 8 bytes of this span, returning them as a <see cref="long"/>. The span
        /// will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 8 bytes of this span as a <see cref="long"/></returns>
        public long ReadLong()
        {
            var tempSpan = span.ReadBytesAsSpan(sizeof(long));
            var temp = BitConverter.ToInt64(tempSpan);
            return BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
        }

        /// <summary>
        /// Consume the first 4 bytes of this span, returning them as a <see cref="float"/>. The
        /// span will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 4 bytes of this span as a <see cref="float"/></returns>
        public float ReadFloat()
        {
            var tempSpan = span.ReadBytesAsSpan(sizeof(float));
            var floatAsInt = BitConverter.ToInt32(tempSpan);
            return BitConverter.IsLittleEndian
                ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(floatAsInt))
                : BitConverter.Int32BitsToSingle(floatAsInt);
        }

        /// <summary>
        /// Consume the first 8 bytes of this span, returning them as a <see cref="double"/>. The
        /// span will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 8 bytes of this span as a <see cref="double"/></returns>
        public double ReadDouble()
        {
            var tempSpan = span.ReadBytesAsSpan(sizeof(double));
            var doubleAsLong = BitConverter.ToInt64(tempSpan);
            return BitConverter.IsLittleEndian
                ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(doubleAsLong))
                : BitConverter.Int64BitsToDouble(doubleAsLong);
        }

        /// <summary>
        /// Consume the desired number of bytes as a new span. The span will now point to all bytes
        /// after the consumed bytes.
        /// </summary>
        /// <param name="length"></param>
        /// <returns>A span of the desired bytes</returns>
        public ReadOnlySpan<byte> ReadBytesAsSpan(int length)
        {
            var result = span[..length];
            span = span[length..];
            return result;
        }

        /// <summary>
        /// Consume all remaining bytes as a new <see cref="byte"/> array. The span will now point
        /// to an empty span.
        /// </summary>
        /// <returns>A new byte array with all bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadBytes()
        {
            var result = span.ToArray();
            span = default;
            return result;
        }

        /// <summary>
        /// Consume the desired number of bytes as a new <see cref="byte"/> array. The span will now
        /// point to all bytes after the consumed bytes.
        /// </summary>
        /// <param name="length"></param>
        /// <returns>A new byte array with the desired bytes</returns>
        public byte[] ReadBytes(int length)
        {
            var result = new byte[length];
            span[..length].CopyTo(result.AsSpan());
            span = span[length..];
            return result;
        }

        /// <summary>
        /// Consume all remaining bytes as a UTF-8 character string. The span will now point to an
        /// empty span.
        /// </summary>
        /// <returns>UTF-8 character string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
        {
            var result = Charsets.Default.GetString(span);
            span = default;
            return result;
        }

        /// <summary>
        /// Read the desired number of bytes as a UTF-8 character string. If the number of bytes
        /// specified will not result in a valid UTF-8 string (e.g. ends with a continuation
        /// character), this method will fail. The span will now point to all bytes after the
        /// consumed bytes.
        /// </summary>
        /// <param name="length">Number of bytes to convert to a string</param>
        /// <returns>UTF-8 character string</returns>
        public string ReadString(int length)
        {
            var result = Charsets.Default.GetString(span[..length]);
            span = span[length..];
            return result;
        }

        /// <summary>
        /// Read as many characters as needed until the buffer contains a null terminator. If the
        /// entire buffer is the string, but it does not end with a null terminator then the method
        /// will fail.
        /// </summary>
        /// <returns>Next available null terminated string from the buffer</returns>
        public string ReadCString()
        {
            var index = 0;
            while (index < span.Length && span[index++] != 0) ;
            var result = Charsets.Default.GetString(span[..(index - 1)]);
            span = index == span.Length ? default : span[index..];
            return result;
        }
    }

    extension(IBufferWriter<byte> bufferWriter)
    {
        /// <summary>
        /// Write a single <see cref="byte"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteByte(byte value)
        {
            var span = bufferWriter.GetSpan();
            span[0] = value;
            bufferWriter.Advance(sizeof(byte));
        }

        /// <summary>
        /// Write a single <see cref="short"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteShort(short value)
        {
            var span = bufferWriter.GetSpan(sizeof(short));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            BitConverter.TryWriteBytes(span, value);
            bufferWriter.Advance(sizeof(short));
        }

        /// <summary>
        /// Write a single <see cref="int"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt(int value)
        {
            var span = bufferWriter.GetSpan(sizeof(int));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            BitConverter.TryWriteBytes(span, value);
            bufferWriter.Advance(sizeof(int));
        }

        /// <summary>
        /// Write a single <see cref="uint"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt(uint value)
        {
            var span = bufferWriter.GetSpan(sizeof(uint));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            BitConverter.TryWriteBytes(span, value);
            bufferWriter.Advance(sizeof(uint));
        }

        /// <summary>
        /// Write a single <see cref="long"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteLong(long value)
        {
            var span = bufferWriter.GetSpan(sizeof(long));
            if (BitConverter.IsLittleEndian)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            BitConverter.TryWriteBytes(span, value);
            bufferWriter.Advance(sizeof(long));
        }

        /// <summary>
        /// Write a single <see cref="float"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteFloat(float value)
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

            bufferWriter.Advance(sizeof(float));
        }

        /// <summary>
        /// Write a single <see cref="double"/> to this writer 
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteDouble(double value)
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

            bufferWriter.Advance(sizeof(double));
        }

        /// <summary>
        /// Write all <see cref="char"/>s to this writer. Uses <see cref="Charsets.Default"/> to
        /// encode characters to bytes.
        /// </summary>
        /// <param name="value">String to write</param>
        public void WriteString(ReadOnlySpan<char> value)
        {
            var size = Charsets.Default.GetByteCount(value);
            var span = bufferWriter.GetSpan(size);
            Charsets.Default.GetBytes(value, span);
            bufferWriter.Advance(size);
        }

        /// <summary>
        /// Write the specified chars with a null termination to replicate a CString. Uses
        /// <see cref="Charsets.Default"/> to encode characters to bytes.
        /// </summary>
        /// <param name="value">String to write</param>
        public void WriteCString(ReadOnlySpan<char> value)
        {
            if (value.Length != 0)
            {
                bufferWriter.WriteString(value);
            }

            bufferWriter.WriteByte(0);
        }

        /// <summary>
        /// <para>
        /// Perform the supplied action against this buffer writer while calculating and writing the
        /// total number of bytes written as a <see cref="int"/> prefix.
        /// </para>
        /// <para>
        /// This is a general method for all buffer writers but does have a special case for
        /// <see cref="ArrayBufferWriter"/> to avoid copying to a temp buffer before performing the
        /// final write. A temp buffer is needed in other cases because there is no way to reliably
        /// write a prefix and update it later with the actual number of bytes written. Therefore,
        /// we must first perform the write action to a temp buffer, write that size to the target
        /// buffer and finally copy all bytes to the target buffer. In the case where the target
        /// buffer is a <see cref="ArrayBufferWriter"/> there is custom functionality to write
        /// length prefix so we defer to that instead.
        /// </para>
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
            ArgumentNullException.ThrowIfNull(writeAction);
            if (bufferWriter is ArrayBufferWriter wb)
            {
                var startLocation = wb.StartWritingLengthPrefixed();
                writeAction(wb);
                wb.FinishWritingLengthPrefixed(startLocation, includeLength);
                return;
            }

            using ArrayBufferWriter tempWriter = new(initialCapacity: 1024);
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
}
