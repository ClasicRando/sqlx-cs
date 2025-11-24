using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("sqlx-cs-pg")]
[assembly: InternalsVisibleTo("sqlx-cs-core-test")]
namespace Sqlx.Core.Buffer;

/// <summary>
/// Writable buffer of <see cref="byte"/>s that is always backed by a shared array pool value. Bytes
/// written this buffer and will continue to be written to the end of the buffer until
/// <see cref="Reset"/> is called. To use the written bytes, reference <see cref="ReadableMemory"/>.
/// To ensure that the <see cref="ArrayPool{T}.Shared"/> pool is not exhausted you must dispose of
/// each instance created which calls <see cref="ArrayPool{T}.Return"/>.
/// </summary>
public sealed class WriteBuffer : IBufferWriter<byte>, IDisposable
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
    private int Remaining => _buffer.Length - _writePosition;

    /// <summary><see cref="ReadOnlyMemory{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlyMemory<byte> ReadableMemory => _buffer.AsMemory(0, _writePosition);

    /// <summary><see cref="ReadOnlySpan{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlySpan<byte> ReadableSpan => _buffer.AsSpan(0, _writePosition);

    internal int StartWritingLengthPrefixed()
    {
        var lenghtPrefixStart = _writePosition;
        WriteInt(0);
        return lenghtPrefixStart;
    }

    internal void FinishWritingLengthPrefixed(int lenghtPrefixStart, bool includeLength)
    {
        var previousWritePosition = _writePosition;
        var length = _writePosition - lenghtPrefixStart - (includeLength ? 0 : 4);
        _writePosition = lenghtPrefixStart;
        WriteInt(length);
        _writePosition = previousWritePosition;
    }

    public void WriteByte(byte value)
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

    public void WriteUInt(uint value)
    {
        CheckBound(sizeof(uint));
        Unsafe.WriteUnaligned(
            ref _buffer[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(uint);
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

    public void Reset()
    {
        _writePosition = 0;
    }

    public void Dispose()
    {
        ArrayPool.Return(_buffer);
        _buffer = [];
        _writePosition = -1;
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
}
