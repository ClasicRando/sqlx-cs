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
        /// Read a byte from this sequence and return the remaining sequence
        /// </summary>
        /// <returns>Byte read from the sequence</returns>
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
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public void ReadBytes(in Span<byte> destination)
        {
            var slice = sequence.Slice(0, destination.Length);
            slice.CopyTo(destination);
            sequence = sequence.Slice(destination.Length);
        }

        /// <summary>
        /// Read all remaining bytes as a UTF-8 character string
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
        /// Read the desired number of bytes as a UTF-8 character string. If the number of bytes
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
        /// Read as many characters as needed until the buffer contains a null terminator. If the
        /// entire buffer is the string, but it does not end with a null terminator then the method
        /// will fail.
        /// </summary>
        /// <returns>Next available null terminated string from the buffer</returns>
        public string ReadCString()
        {
            var reader = new SequenceReader<byte>(sequence);
            ReadOnlySequence<byte> slice;
            SequencePosition nextStart;
            if (reader.TryReadTo(out ReadOnlySequence<byte> temp, (byte)'\0'))
            {
                slice = temp;
                nextStart = reader.Position;
            }
            else
            {
                slice = sequence;
                nextStart = sequence.End;
            }

            var result = Charsets.Default.GetString(slice);
            sequence = sequence.Slice(nextStart);
            return result;
        }
    }
}
