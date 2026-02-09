using System.Buffers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("sqlx-cs-pg")]
[assembly: InternalsVisibleTo("sqlx-cs-core-test")]
namespace Sqlx.Core.Buffer;

/// <summary>
/// Writable buffer of <see cref="byte"/>s that is always backed by a shared array pool value. Bytes
/// written this buffer and will continue to be written to the end of the buffer until
/// <see cref="Reset"/> is called. To use the written bytes, reference <see cref="ReadableSpan"/>.
/// To ensure that the <see cref="ArrayPool{T}.Shared"/> pool is not exhausted you must dispose of
/// each instance created which calls <see cref="ArrayPool{T}.Return"/>.
/// </summary>
public sealed class PooledArrayBufferWriter : IBufferWriter<byte>, IDisposable
{
    private const int DefaultCapacity = 8192;
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    
    private byte[] _buffer;

    public PooledArrayBufferWriter(int initialCapacity = DefaultCapacity)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("Buffer size must be greater than 0", nameof(initialCapacity));
        }
        _buffer = ArrayPool.Rent(initialCapacity);
    }

    public int WrittenCount { get; private set; }

    /// <summary><see cref="ReadOnlySpan{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlySpan<byte> ReadableSpan
    {
        get
        {
            return WrittenCount switch
            {
                < 0 => throw new InvalidOperationException($"Negative written count: {WrittenCount}. Probably disposed?"),
                0 => default,
                _ when WrittenCount > _buffer.Length => throw new InvalidOperationException(
                    $"Written count is beyond buffer size. Buffer Size: {_buffer.Length}, Written Count: {WrittenCount}: Bytes: [{string.Join(",", _buffer)}]"),
                _ => _buffer.AsSpan(0, WrittenCount),
            };
        }
    }

    internal int StartWritingLengthPrefixed()
    {
        var lenghtPrefixStart = WrittenCount;
        this.WriteInt(0);
        return lenghtPrefixStart;
    }

    internal void FinishWritingLengthPrefixed(int lenghtPrefixStart, bool includeLength)
    {
        var previousWritePosition = WrittenCount;
        var length = WrittenCount - lenghtPrefixStart - (includeLength ? 0 : 4);
        WrittenCount = lenghtPrefixStart;
        this.WriteInt(length);
        WrittenCount = previousWritePosition;
    }

    private void CheckAndResizeBuffer(int sizeHint)
    {
        sizeHint = sizeHint switch
        {
            < 0 => throw new ArgumentException("Size must be positive or zero", nameof(sizeHint)),
            0 => 1,
            _ => sizeHint,
        };

        if (sizeHint <= _buffer.Length - WrittenCount)
        {
            return;
        }
        
        var currentLength = _buffer.Length;
        var growBy = int.Max(sizeHint, currentLength);
        var newSize = currentLength + growBy;
        var newBuffer = ArrayPool.Rent(newSize);
        _buffer.AsSpan(0, WrittenCount).CopyTo(newBuffer);
        ArrayPool.Return(_buffer);
        _buffer = newBuffer;
    }

    public void Reset()
    {
        WrittenCount = 0;
    }

    public void Dispose()
    {
        ArrayPool.Return(_buffer);
        _buffer = [];
        WrittenCount = -1;
    }
    
    // IBufferWriter<byte> implementation

    public void Advance(int count)
    {
        WrittenCount += count;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return sizeHint == 0
            ? _buffer.AsSpan(WrittenCount)
            : _buffer.AsSpan(WrittenCount, sizeHint);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return sizeHint == 0
            ? _buffer.AsMemory(WrittenCount)
            : _buffer.AsMemory(WrittenCount, sizeHint);
    }
}
