using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sqlx.Core.Buffer;

public ref struct WriteMemory : IBufferWriter<byte>
{
    private static readonly Encoding Encoding = Charsets.Default;
    
    private Memory<byte> _buffer;
    private int _writePosition;

    public WriteMemory(Memory<byte> buffer)
    {
        _buffer = buffer;
    }

    public WriteMemory(byte[] bytes) : this(bytes.AsMemory()) {}

    private int Remaining => _buffer.Length - _writePosition;

    public ReadOnlyMemory<byte> ReadableMemory => _buffer[.._writePosition];

    public void WriteByte(byte value)
    {
        CheckBound(sizeof(byte));
        _buffer.Span[_writePosition] = value;
        _writePosition += sizeof(byte);
    }

    public void WriteShort(short value)
    {
        CheckBound(sizeof(short));
        Unsafe.WriteUnaligned(
            ref _buffer.Span[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(short);
    }

    public void WriteInt(int value)
    {
        CheckBound(sizeof(int));
        Unsafe.WriteUnaligned(
            ref _buffer.Span[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(int);
    }

    public void WriteLong(long value)
    {
        CheckBound(sizeof(long));
        Unsafe.WriteUnaligned(
            ref _buffer.Span[_writePosition],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value);
        _writePosition += sizeof(long);
    }

    public void WriteFloat(float value)
    {
        CheckBound(sizeof(float));

        if (BitConverter.IsLittleEndian)
        {
            Unsafe.WriteUnaligned(
                ref _buffer.Span[_writePosition],
                BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value)));
        }
        else
        {
            Unsafe.WriteUnaligned(ref _buffer.Span[_writePosition], value);
        }
        _writePosition += sizeof(float);
    }

    public void WriteDouble(double value)
    {
        CheckBound(sizeof(double));

        if (BitConverter.IsLittleEndian)
        {
            Unsafe.WriteUnaligned(
                ref _buffer.Span[_writePosition],
                BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value)));
        }
        else
        {
            Unsafe.WriteUnaligned(ref _buffer.Span[_writePosition], value);
        }
        _writePosition += sizeof(double);
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        CheckBound(bytes.Length);
        bytes.CopyTo(_buffer.Span.Slice(_writePosition, _buffer.Length - _writePosition));
        _writePosition += bytes.Length;
    }

    public void WriteBytes(ReadOnlyMemory<byte> bytes)
    {
        WriteBytes(bytes.Span);
    }

    public Span<byte> WriteToSpan(int length)
    {
        var span = _buffer.Slice(_writePosition, length);
        _writePosition += length;
        return span.Span;
    }

    public void WriteString(ReadOnlySpan<char> value)
    {
        CheckBound(Encoding.GetByteCount(value));
        _writePosition += Encoding.GetBytes(
            value,
            _buffer.Span.Slice(_writePosition, _buffer.Length - _writePosition));
    }

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

    public delegate void WriteAction(WriteMemory buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLengthPrefixed(bool includeLength, WriteAction action)
    {
        var start = _writePosition;
        WriteInt(0);
        action(this);
        var size = _writePosition - start - (includeLength ? 0 : 4);
        Unsafe.WriteUnaligned(
            ref _buffer.Span[start],
            BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(size) : size);
    }

    public byte[] CopyBytes()
    {
        var result = new byte[_writePosition];
        _buffer[.._writePosition].CopyTo(result);
        return result;
    }

    public void Reset()
    {
        _writePosition = 0;
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
            ? _buffer[_writePosition..].Span
            : _buffer.Slice(_writePosition, sizeHint).Span;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        CheckBound(sizeHint);
        return sizeHint == 0
            ? _buffer[_writePosition..]
            : _buffer.Slice(_writePosition, sizeHint);
    }
}
