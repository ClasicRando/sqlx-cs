using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sqlx.Core.Buffer;

public sealed class WriteBuffer : System.IO.Stream, IDisposable
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

    public int Remaining => _buffer.Length - _writePosition;

    public int BytesWritten => _writePosition;

    public ReadOnlySpan<byte> Span => _buffer.AsSpan(0, _writePosition);

    public ReadOnlyMemory<byte> Memory => _buffer.AsMemory(0, _writePosition);

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

    public Span<byte> WriteToSpan(int length)
    {
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

    public void WriteCString(ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLengthPrefixed(bool includeLength, Action<WriteBuffer> action)
    {
        var start = _writePosition;
        var size = includeLength ? 4 : 0;
        WriteInt(0);
        action(this);
        size += _writePosition - start;
        Unsafe.WriteUnaligned(ref _buffer[start], size);
    }

    public byte[] CopyBytes()
    {
        var result = new byte[_writePosition];
        _buffer.AsSpan(0, _writePosition).CopyTo(result);
        return result;
    }

    public new void Dispose()
    {
        ArrayPool.Return(_buffer);
        _buffer = [];
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
