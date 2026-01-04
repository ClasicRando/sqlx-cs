using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Sqlx.Core.Exceptions;

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
            if (span.Length < 1)
            {
                throw new IndexOutOfRangeException("Span must not be empty");
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
            var temp = BitConverter.ToInt16(span[..sizeof(short)]);
            var result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            span = span[sizeof(short)..];
            return result;
        }

        /// <summary>
        /// Consume the first 4 bytes of this span, returning them as a <see cref="int"/>. The span
        /// will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 4 bytes of this span as a <see cref="int"/></returns>
        public int ReadInt()
        {
            var temp = BitConverter.ToInt32(span[..sizeof(int)]);
            var result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            span = span[sizeof(int)..];
            return result;
        }

        /// <summary>
        /// Consume the first 4 bytes of this span, returning them as a <see cref="uint"/>. The span
        /// will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 4 bytes of this span as a <see cref="uint"/></returns>
        public uint ReadUInt()
        {
            var temp = BitConverter.ToUInt32(span[..sizeof(uint)]);
            var result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            span = span[sizeof(uint)..];
            return result;
        }

        /// <summary>
        /// Consume the first 8 bytes of this span, returning them as a <see cref="long"/>. The span
        /// will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 8 bytes of this span as a <see cref="long"/></returns>
        public long ReadLong()
        {
            var temp = BitConverter.ToInt64(span[..sizeof(long)]);
            var result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            span = span[sizeof(long)..];
            return result;
        }
        
        /// <summary>
        /// Consume the first 4 bytes of this span, returning them as a <see cref="float"/>. The
        /// span will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 4 bytes of this span as a <see cref="float"/></returns>
        public float ReadFloat()
        {
            var result = BitConverter.IsLittleEndian
                ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(span)))
                : BitConverter.Int32BitsToSingle(BitConverter.ToInt32(span));
            span = span[sizeof(float)..];
            return result;
        }

        /// <summary>
        /// Consume the first 8 bytes of this span, returning them as a <see cref="double"/>. The
        /// span will now point to all bytes after the consumed bytes.
        /// </summary>
        /// <returns>First 8 bytes of this span as a <see cref="double"/></returns>
        public double ReadDouble()
        {
            var result = BitConverter.IsLittleEndian
                ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(BitConverter.ToInt64(span)))
                : BitConverter.Int64BitsToDouble(BitConverter.ToInt64(span));
            span = span[sizeof(double)..];
            return result;
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
        public byte[] ReadBytes() => span.ReadBytes(span.Length);

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
        public string ReadString() => span.ReadString(span.Length);

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
            while (span[index++] != 0);
            var result = Charsets.Default.GetString(span[..(index - 1)]);
            span = span[(index + 1)..];
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
        /// <see cref="PooledArrayBufferWriter"/> to avoid copying to a temp buffer before
        /// performing the final write. A temp buffer is needed in other cases because there is no
        /// way to reliably write a prefix and update it later with the actual number of bytes
        /// written. Therefore, we must first perform the write action to a temp buffer, write that
        /// size to the target buffer and finally copy all bytes to the target buffer. In the case
        /// where the target buffer is a <see cref="PooledArrayBufferWriter"/> there is custom
        /// functionality to write length prefix so we defer to that instead.
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
            if (bufferWriter is PooledArrayBufferWriter wb)
            {
                var startLocation = wb.StartWritingLengthPrefixed();
                writeAction(wb);
                wb.FinishWritingLengthPrefixed(startLocation, includeLength);
                return;
            }

            using PooledArrayBufferWriter tempWriter = new(initialCapacity: 1024);
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

    extension(PipeReader reader)
    {
        /// <summary>
        /// Read the next <see cref="byte"/> from the stream
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The next byte from the stream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            ReadResult readResult = await reader.ReadAtLeastAsync(sizeof(byte), cancellationToken)
                .ConfigureAwait(false);
            var buffer = readResult.Buffer;
            if (buffer.Length < sizeof(byte))
            {
                throw new SqlxException("Attempted to read after stream was closed");
            }

            var result = buffer.FirstSpan[0];
            reader.AdvanceTo(buffer.GetPosition(sizeof(byte)));
            return result;
        }

        /// <summary>
        /// Read the next 4 bytes from the stream and combine into a single <see cref="int"/>
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation</param>
        /// <returns>The next integer from the stream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<int> ReadIntAsync(CancellationToken cancellationToken)
        {
            ReadResult readResult = await reader.ReadAtLeastAsync(sizeof(int), cancellationToken)
                .ConfigureAwait(false);
            var buffer = readResult.Buffer;
            if (buffer.Length < sizeof(int))
            {
                throw new SqlxException("Attempted to read after stream was closed");
            }

            var result = buffer.ReadInt();
            reader.AdvanceTo(buffer.Start);
            return result;
        }
    }


    [DoesNotReturn]
    private static void ThrowSequenceExhausted()
    {
        throw new SqlxException("Not enough bytes available. Please submit bug report.");
    }

    extension(ref ReadOnlySequence<byte> sequence)
    {
        /// <summary>
        /// Consume the first byte from this sequence
        /// </summary>
        /// <returns>First byte from the sequence</returns>
        public byte ReadByte()
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!reader.TryRead(out var result))
            {
                ThrowSequenceExhausted();
            }

            sequence = sequence.Slice(sizeof(byte));
            return result;
        }

        /// <summary>
        /// Consume the first 2 bytes from this sequence as a <see cref="short"/>
        /// </summary>
        /// <returns>First <see cref="short"/> from the sequence</returns>
        public short ReadShort()
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!reader.TryReadBigEndian(out short result))
            {
                ThrowSequenceExhausted();
            }

            sequence = sequence.Slice(sizeof(short));
            return result;
        }

        /// <summary>
        /// Consume the first 4 bytes from this sequence as a <see cref="int"/>
        /// </summary>
        /// <returns>First <see cref="int"/> from the sequence</returns>
        public int ReadInt()
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!reader.TryReadBigEndian(out int result))
            {
                ThrowSequenceExhausted();
            }

            sequence = sequence.Slice(sizeof(int));
            return result;
        }

        /// <summary>
        /// Consume the first 4 bytes from this sequence as a <see cref="uint"/>
        /// </summary>
        /// <returns>First <see cref="uint"/> from the sequence</returns>
        public uint ReadUInt()
        {
            uint temp;
            if (sequence.IsSingleSegment)
            {
                var span = sequence.FirstSpan;
                temp = BitConverter.ToUInt32(span[..sizeof(uint)]);
            }
            else
            {
                Span<byte> tempSpan = stackalloc byte[sizeof(uint)];
                sequence.CopyTo(tempSpan);
                temp = BitConverter.ToUInt32(tempSpan[..sizeof(uint)]);
            }

            var result = BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(temp)
                : temp;
            sequence = sequence.Slice(sizeof(uint));
            return result;
        }

        /// <summary>
        /// Consume bytes from this sequence and copy them to the destination
        /// </summary>
        /// <param name="destination">Target of the byte reading</param>
        public void ReadBytes(in Span<byte> destination)
        {
            var slice = sequence.Slice(0, destination.Length);
            slice.CopyTo(destination);
            sequence = sequence.Slice(destination.Length);
        }

        /// <summary>
        /// Consume all remaining bytes as a UTF-8 character string
        /// </summary>
        /// <returns>UTF-8 character string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
        {
            var result = Charsets.Default.GetString(sequence);
            sequence = sequence.Slice(sequence.End);
            return result;
        }

        /// <summary>
        /// Consume the desired number of bytes as a UTF-8 character string. If the number of bytes
        /// specified will not result in a valid UTF-8 string (e.g. ends with a continuation
        /// character), this method will fail.
        /// </summary>
        /// <param name="length">Number of bytes to convert to a string</param>
        /// <returns>String read from the sequence</returns>
        public string ReadString(int length)
        {
            var slice = sequence.Slice(0, length);
            var result = Charsets.Default.GetString(slice);
            sequence = sequence.Slice(length);
            return result;
        }

        /// <summary>
        /// Consume as many characters as needed until the buffer contains a null terminator. If the
        /// entire buffer is the string, but it does not end with a null terminator then the method
        /// will fail.
        /// </summary>
        /// <returns>Next available null terminated string from the buffer</returns>
        public string ReadCString()
        {
            var reader = new SequenceReader<byte>(sequence);
            if (!reader.TryReadTo(out ReadOnlySequence<byte> slice, (byte)'\0'))
            {
                ThrowSequenceExhausted();
            }
            SequencePosition nextStart = reader.Position;

            var result = Charsets.Default.GetString(slice);
            sequence = sequence.Slice(nextStart);
            return result;
        }
    }
}
