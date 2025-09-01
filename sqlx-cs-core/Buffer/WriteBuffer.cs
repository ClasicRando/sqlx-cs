using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sqlx.Core.Buffer;

/// <summary>
/// Writable buffer of <see cref="byte"/>s that is always backed by a shared array pool value. Bytes
/// written this buffer and will continue to be written to the end of the buffer until
/// <see cref="Reset"/> is called. To use the written bytes, reference <see cref="ReadableMemory"/>.
/// To ensure that the <see cref="ArrayPool{T}.Shared"/> pool is not exhausted you must dispose of
/// each instance created which calls <see cref="ArrayPool{T}.Return"/>.
/// </summary>
public sealed class WriteBuffer : System.IO.Stream, IBufferWriter<byte>, IDisposable
{
    private const int DefaultCapacity = 8192;
    private static readonly Encoding Encoding = Charsets.Default;
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    
    private byte[] _buffer;
    private int _writePosition;

    public WriteBuffer(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Buffer size must be greater than 0", nameof(capacity));
        }
        _buffer = ArrayPool.Rent(capacity);
    }

    /// <summary>Number writeable bytes remaining in the buffer</summary>
    public int Remaining => _buffer.Length - _writePosition;

    /// <summary><see cref="ReadOnlyMemory{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlyMemory<byte> ReadableMemory => _buffer.AsMemory(0, _writePosition);

    /// <summary><see cref="ReadOnlySpan{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlySpan<byte> ReadableSpan => _buffer.AsSpan(0, _writePosition);

    public new void WriteByte(byte value)
    {
        CheckBound(sizeof(byte));
        _buffer[_writePosition] = value;
        _writePosition += sizeof(byte);
    }

    public void WriteShort(short value)
    {
        CheckBound(sizeof(short));
        Unsafe.WriteUnaligned(
            ref _buffer[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(short);
    }

    public void WriteInt(int value)
    {
        CheckBound(sizeof(int));
        Unsafe.WriteUnaligned(
            ref _buffer[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(int);
    }

    public void WriteLong(long value)
    {
        CheckBound(sizeof(long));
        Unsafe.WriteUnaligned(
            ref _buffer[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(long);
    }

    public void WriteFloat(float value)
    {
        CheckBound(sizeof(float));
        if (BitConverter.IsLittleEndian)
        {
            Unsafe.WriteUnaligned(
                ref _buffer[_writePosition],
                BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value)));
        }
        else
        {
            Unsafe.WriteUnaligned(ref _buffer[_writePosition], value);
        }
        _writePosition += sizeof(float);
    }

    public void WriteDouble(double value)
    {
        CheckBound(sizeof(double));
        if (BitConverter.IsLittleEndian)
        {
            Unsafe.WriteUnaligned(
                ref _buffer[_writePosition],
                BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value)));
        }
        else
        {
            Unsafe.WriteUnaligned(ref _buffer[_writePosition], value);
        }
        _writePosition += sizeof(double);
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        CheckBound(bytes.Length);
        bytes.CopyTo(_buffer.AsSpan(_writePosition, _buffer.Length - _writePosition));
        _writePosition += bytes.Length;
    }

    public void WriteBytes(ReadOnlyMemory<byte> bytes)
    {
        WriteBytes(bytes.Span);
    }

    /// <summary>
    /// <para>
    /// Get a writeable <see cref="Span{T}"/> of the internal buffer for a specified length and move
    /// the write position of the buffer to the index after the span.
    /// </para>
    /// <para>
    /// Calls to this method must write new values to each index in the span or reset the index to
    /// the default (0). This is due the shared nature of the underlining data and arrays returned
    /// to the shared pool might not have been cleared beforehand. If no value is written to the
    /// span's index, the previous value would be preserved and possibly disrupt your intended
    /// value.
    /// </para>
    /// </summary>
    /// <param name="length">span length to get for writing</param>
    /// <returns><see cref="Span{T}"/> over the underlining buffer with the desired length</returns>
    public Span<byte> WriteToSpan(int length)
    {
        CheckBound(length);
        var span = _buffer.AsSpan(_writePosition, length);
        _writePosition += length;
        return span;
    }

    public void WriteString(ReadOnlySpan<char> value)
    {
        CheckBound(Encoding.GetByteCount(value));
        _writePosition += Encoding.GetBytes(
            value,
            _buffer.AsSpan(_writePosition, _buffer.Length - _writePosition));
    }

    /// <summary>
    /// Write the specified chars with a null termination to replicate a CString
    /// </summary>
    /// <param name="value">string to write</param>
    public void WriteCString(ReadOnlySpan<char> value)
    {
        if (value.Length != 0)
        {
            WriteString(value);
        }
        WriteByte(0);
    }

    [Conditional("DEBUG")]
    private void CheckBound(int size)
    {
        if (size > Remaining)
        {
            throw new InvalidOperationException("Attempted to write to buffer outside of writable space");
        }
    }

    /// <summary>
    /// Perform a Write action against this buffer where the byte count written by the action is
    /// encoded before the write action.
    /// </summary>
    /// <param name="includeLength">
    /// true if the length integer written before the action includes the length size (4), false if
    /// the number of bytes written is the total size
    /// </param>
    /// <param name="action">arbitrary write action performed against this buffer</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLengthPrefixed(bool includeLength, Action<WriteBuffer> action)
    {
        var start = _writePosition;
        WriteInt(0);
        action(this);
        var size = _writePosition - start - (includeLength ? 0 : 4);
        Unsafe.WriteUnaligned(
            ref _buffer[start],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(size) : size);
    }

    public void Reset()
    {
        _writePosition = 0;
    }

    public new void Dispose()
    {
        ArrayPool.Return(_buffer);
        _buffer = [];
    }
    
    // IBufferWriter<byte> implementation

    public void Advance(int count)
    {
        _writePosition += count;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        CheckBound(sizeHint);
        return sizeHint == 0
            ? _buffer.AsSpan(_writePosition)
            : _buffer.AsSpan(_writePosition, sizeHint);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        CheckBound(sizeHint);
        return sizeHint == 0
            ? _buffer.AsMemory(_writePosition)
            : _buffer.AsMemory(_writePosition, sizeHint);
    }

    // Stream implementations. Does not allow seeking, reading or setting length

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _buffer.Length;

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteBytes(buffer.AsSpan(offset, count));
    }

    public override long Position
    {
        get => _writePosition;
        set => _writePosition += (int)value;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
