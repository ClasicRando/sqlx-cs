using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sqlx.Core.Buffer;

/// <summary>
/// Read-only buffer over a <see cref="Span{T}"/> of <see cref="byte"/>s. This acts as a thin
/// wrapper over those bytes to allow for reading binary data (or text data as bytes) in an
/// efficient manner without creating new objects to be collected. To avoid unnecessary checks at
/// runtime, bounds checks are only done in DEBUG mode. 
/// </summary>
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

    /// <summary>
    /// Skip the desired numbers of bytes in the buffer
    /// </summary>
    /// <param name="count">number of bytes to skip</param>
    /// <returns>a range of the skipped indexes</returns>
    public Range Skip(int count)
    {
        CheckBound(count);
        var start = _position;
        _position += count;
        return new Range(start, _position);
    }

    public byte ReadByte()
    {
        CheckBound(sizeof(byte));
        var result = _inner[_position];
        _position += sizeof(byte);
        return result;
    }

    public short ReadShort()
    {
        CheckBound(sizeof(short));
        var result = Unsafe.ReadUnaligned<short>(ref _inner[_position]);
        _position += sizeof(short);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    public int ReadInt()
    {
        CheckBound(sizeof(int));
        var result = Unsafe.ReadUnaligned<int>(ref _inner[_position]);
        _position += sizeof(int);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    public long ReadLong()
    {
        CheckBound(sizeof(long));
        var result = Unsafe.ReadUnaligned<long>(ref _inner[_position]);
        _position += sizeof(long);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(result) : result;
    }

    public float ReadFloat()
    {
        CheckBound(sizeof(float));
        var result = BitConverter.IsLittleEndian
            ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref _inner[_position])))
            : Unsafe.ReadUnaligned<float>(ref _inner[_position]);
        _position += sizeof(float);
        return result;
    }

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
    public ReadOnlySpan<byte> ReadBytesAsSpan() => ReadBytesAsSpan(Remaining);

    public ReadOnlySpan<byte> ReadBytesAsSpan(int length)
    {
        CheckBound(length);
        var result = _inner.Slice(_position, length);
        _position += length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes() => ReadBytes(Remaining);

    public byte[] ReadBytes(int length)
    {
        CheckBound(length);
        var result = new byte[length];
        _inner.Slice(_position, length).CopyTo(result.AsSpan());
        _position += length;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadText() => ReadText(Remaining);

    /// <summary>
    /// Read the desired number of bytes as a UTF-8 character string. If the number of bytes
    /// specified will not result in a valid UTF-8 string (e.g. ends with a continuation character),
    /// this method will fail.
    /// </summary>
    /// <param name="length">number of bytes to convert to a string</param>
    /// <returns>UTF-8 character string</returns>
    public string ReadText(int length)
    {
        CheckBound(length);
        var result = Encoding.GetString(_inner.Slice(_position, length));
        _position += length;
        return result;
    }

    /// <summary>
    /// Read as many characters as needed until the buffer contains a null terminator. If the entire
    /// buffer is the string, but it does not end with a null terminator then the method will fail.
    /// </summary>
    /// <returns>next available null terminated string from the buffer</returns>
    public string ReadCString()
    {
        var index = _position;
        while (_inner[index++] != 0);
        var result = Encoding.GetString(_inner.Slice(_position, index - _position - 1));
        _position += index - _position;
        return result;
    }

    /// <summary>
    /// Take a sub slice of this buffer and treat it as a separate <see cref="ReadBuffer"/>.
    /// Advances this buffer's read position to the end of the extracted slice.
    /// </summary>
    /// <param name="length">number of bytes to contain within the sub slice</param>
    /// <returns>sub slice of this buffer as a new <see cref="ReadBuffer"/></returns>
    public ReadBuffer Slice(int length)
    {
        CheckBound(length);
        var slice = _inner.Slice(_position, length);
        _position += length;
        return new ReadBuffer(slice);
    }

    /// <summary>Set the read position of this buffer to the start</summary>
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
