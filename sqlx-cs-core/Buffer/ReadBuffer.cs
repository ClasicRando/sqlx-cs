using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sqlx.Core.Buffer;

public ref struct ReadBuffer
{
    private static readonly Encoding Encoding = Charsets.Default;
    internal readonly Span<byte> Inner;
    private int _position = 0;

    public ReadBuffer(Span<byte> inner)
    {
        Inner = inner;
    }

    public int Remaining => Inner.Length - _position;

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
        var result = Inner[_position];
        _position += sizeof(byte);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadShort()
    {
        CheckBound(sizeof(short));
        var result = Unsafe.ReadUnaligned<short>(ref Inner[_position]);
        _position += sizeof(short);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt()
    {
        CheckBound(sizeof(int));
        var result = Unsafe.ReadUnaligned<int>(ref Inner[_position]);
        _position += sizeof(int);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadLong()
    {
        CheckBound(sizeof(long));
        var result = Unsafe.ReadUnaligned<long>(ref Inner[_position]);
        _position += sizeof(long);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        CheckBound(sizeof(float));
        var result = BitConverter.IsLittleEndian
            ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref Inner[_position])))
            : Unsafe.ReadUnaligned<float>(ref Inner[_position]);
        _position += sizeof(float);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        CheckBound(sizeof(double));
        var result = BitConverter.IsLittleEndian
            ? BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref Inner[_position])))
            : Unsafe.ReadUnaligned<double>(ref Inner[_position]);
        _position += sizeof(double);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytesAsSpan()
    {
        var result = Inner[_position..];
        _position = Inner.Length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytesAsSpan(int length)
    {
        CheckBound(length);
        var result = Inner.Slice(_position, length);
        _position += length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes()
    {
        var result = new byte[Remaining];
        Inner.CopyTo(result.AsSpan());
        _position = Inner.Length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(int length)
    {
        CheckBound(length);
        var result = new byte[Remaining];
        Inner.CopyTo(result.AsSpan());
        _position += length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadText()
    {
        var result = Encoding.GetString(Inner[_position..]);
        _position = Inner.Length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadText(int length)
    {
        CheckBound(length);
        var result = Encoding.GetString(Inner.Slice(_position, length));
        _position += length;
        return result;
    }

    public int GetRemainingCharCount()
    {
        return Encoding.GetCharCount(Inner[_position..]);
    }

    public void ReadText(Span<char> chars)
    {
        CheckBound(chars.Length);
        Encoding.GetChars(Inner.Slice(_position, chars.Length), chars);
        _position += chars.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadCString()
    {
        var index = _position;
        while (Inner[index++] != 0);
        var result = Encoding.GetString(Inner.Slice(_position, index - _position - 1));
        _position += index - _position;
        return result;
    }

    public ReadBuffer Slice(int length)
    {
        CheckBound(length);
        var slice = Inner.Slice(_position, length);
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
