using System.Buffers;

namespace Sqlx.Core.Buffer;

/// <summary>
/// Writable buffer of <see cref="byte"/>s that is backed by an array that may be from the shared
/// pool of arrays if specified. Bytes written this buffer and will continue to be written to the
/// end of the buffer until <see cref="Clear"/> is called. To use the written bytes, reference
/// <see cref="ReadableSpan"/> or <see cref="ReadableMemory"/>. If your writer uses a shared array,
/// ensure the instance is disposed to return the shared array to the pool for reuse. Otherwise, the
/// pool might become exhausted.
/// </summary>
public sealed class ArrayBufferWriter : IBufferWriter<byte>, IDisposable
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;
    
    private bool _disposed;
    private readonly int _initialCapacity;
    private readonly bool _usePooledArray;
    private byte[] _buffer;
    
    public ArrayBufferWriter(
        int initialCapacity = BufferConstants.DefaultBufferSize,
        bool usePooledArray = true)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException(
                "Buffer size must be greater than 0",
                nameof(initialCapacity));
        }

        _initialCapacity = initialCapacity;
        _usePooledArray = usePooledArray;
        _buffer = usePooledArray ? ArrayPool.Rent(initialCapacity) : new byte[initialCapacity];
    }
    
    public int WrittenCount { get; private set; }

    /// <summary><see cref="ReadOnlySpan{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlySpan<byte> ReadableSpan
    {
        get
        {
            var writtenCount = CheckWrittenCount();
            return writtenCount == 0 ? default : _buffer.AsSpan(0, writtenCount);
        }
    }

    /// <summary><see cref="ReadOnlyMemory{T}"/> of the bytes written to this buffer</summary>
    public ReadOnlyMemory<byte> ReadableMemory
    {
        get
        {
            var writtenCount = CheckWrittenCount();
            return writtenCount == 0 ? default : _buffer.AsMemory(0, writtenCount);
        }
    }

    private int CheckWrittenCount()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var writtenCount = WrittenCount;
        if (writtenCount == 0)
        {
            return 0;
        }

        if (writtenCount > _buffer.Length)
        {
            throw new InvalidOperationException(
                $"Written count is beyond buffer size. Buffer Size: {_buffer.Length}, Written Count: {WrittenCount}: Bytes: [{string.Join(",", _buffer)}]");
        }

        return writtenCount;
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
        var length = previousWritePosition - lenghtPrefixStart - (includeLength ? 0 : 4);
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

        var writtenCount = WrittenCount;
        var bufferLength = _buffer.Length;
        if (sizeHint <= bufferLength - writtenCount)
        {
            return;
        }

        var growBy = int.Max(sizeHint, bufferLength);
        var newSize = bufferLength + growBy;
        var newBuffer = _usePooledArray ? ArrayPool.Rent(newSize) : new byte[newSize];
        _buffer.AsSpan(0, writtenCount).CopyTo(newBuffer);
        if (_usePooledArray) ArrayPool.Return(_buffer);
        _buffer = newBuffer;
    }

    /// <summary>
    /// Prepare the buffer for future writes. This does not actually clear the buffer contents but
    /// resets the writer position to the start of the buffer. Future writes will move the position
    /// forward and override previous writes.
    /// </summary>
    public void Clear()
    {
        WrittenCount = 0;
    }

    /// <summary>
    /// Clears the buffer, then resets the buffer capacity to the initial value if required
    /// </summary>
    public void ResetToInitialCapacity()
    {
        Clear();
        if (_buffer.Length == _initialCapacity) return;

        var newBuffer = _usePooledArray
            ? ArrayPool.Rent(_initialCapacity)
            : new byte[_initialCapacity];
        if (_usePooledArray) ArrayPool.Return(_buffer);
        _buffer = newBuffer;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        if (_usePooledArray) ArrayPool.Return(_buffer);
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
