using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sqlx.Core.Buffer;

public ref struct ReadBuffer
{
    private static readonly Encoding Encoding = Charsets.Default;
    private readonly Span<byte> _inner;
    private int _position = 0;

    public ReadBuffer(Span<byte> inner)
    {
        _inner = inner;
    }

    public int Remaining => _inner.Length - _position;

    public bool IsExhausted => Remaining == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, int) Skip(int count)
    {
        CheckBound(count);
        var start = _position;
        _position += count;
        return (start, _position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Request(int count)
    {
        return Remaining >= count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        CheckBound(sizeof(byte));
        var result = _inner[_position];
        _position += sizeof(byte);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadShort()
    {
        CheckBound(sizeof(short));
        var result = Unsafe.ReadUnaligned<short>(ref _inner[_position]);
        _position += sizeof(short);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt()
    {
        CheckBound(sizeof(int));
        var result = Unsafe.ReadUnaligned<int>(ref _inner[_position]);
        _position += sizeof(int);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadLong()
    {
        CheckBound(sizeof(long));
        var result = Unsafe.ReadUnaligned<long>(ref _inner[_position]);
        _position += sizeof(long);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        CheckBound(sizeof(float));
        var result = BitConverter.IsLittleEndian
            ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref _inner[_position])))
            : Unsafe.ReadUnaligned<float>(ref _inner[_position]);
        _position += sizeof(float);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        CheckBound(sizeof(double));
        var result = BitConverter.IsLittleEndian
            ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref _inner[_position])))
            : Unsafe.ReadUnaligned<double>(ref _inner[_position]);
        _position += sizeof(double);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytesAsSpan()
    {
        var result = _inner[_position..];
        _position = _inner.Length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytesAsSpan(int length)
    {
        CheckBound(length);
        var result = _inner.Slice(_position, length);
        _position += length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes()
    {
        return ReadBytes(Remaining);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(int length)
    {
        CheckBound(length);
        var result = new byte[length];
        _inner.Slice(_position, length).CopyTo(result.AsSpan());
        _position += length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadText()
    {
        var result = Encoding.GetString(_inner[_position..]);
        _position = _inner.Length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadText(int length)
    {
        CheckBound(length);
        var result = Encoding.GetString(_inner.Slice(_position, length));
        _position += length;
        return result;
    }

    public int GetRemainingCharCount()
    {
        return Encoding.GetCharCount(_inner[_position..]);
    }

    public void ReadText(Span<char> chars)
    {
        CheckBound(chars.Length);
        Encoding.GetChars(_inner.Slice(_position, chars.Length), chars);
        _position += chars.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadCString()
    {
        var index = _position;
        while (_inner[index++] != 0);
        var result = Encoding.GetString(_inner.Slice(_position, index - _position - 1));
        _position += index - _position;
        return result;
    }

    public ReadBuffer Slice(int length)
    {
        CheckBound(length);
        var slice = _inner.Slice(_position, length);
        _position += length;
        return new ReadBuffer(slice);
    }

    public void Reset()
    {
        _position = 0;
    }
    
    [Conditional("DEBUG")]
    private void CheckBound(int size)
    {
        if (size > Remaining)
        {
            throw new InvalidOperationException("Attempted to write to buffer outside of writable space");
        }
    }
}
