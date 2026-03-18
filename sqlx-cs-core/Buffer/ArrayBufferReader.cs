using System.Runtime.CompilerServices;

namespace Sqlx.Core.Buffer;

/// <summary>
/// <see cref="IBufferReader"/> implementation backed by an array that can grow in capacity if more
/// space is needed to fit the buffered data. The buffer will stay oversized until
/// <see cref="ResetToInitialCapacity"/> is called.
/// </summary>
internal sealed class ArrayBufferReader : IBufferReader
{
    private bool _disposed;
    private readonly int _initialCapacity;
    private byte[] _readBuffer;
    private int _bufferPosition;
    private int _bufferLength;

    public ArrayBufferReader(int initialCapacity = BufferConstants.DefaultBufferSize)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException(
                "Buffer size must be greater than 0",
                nameof(initialCapacity));
        }

        _initialCapacity = initialCapacity;
        _readBuffer = new byte[initialCapacity];
    }

    public ReadOnlySpan<byte> Span => _readBuffer.AsSpan(_bufferPosition.._bufferLength);

    public ReadOnlyMemory<byte> Memory =>
        _readBuffer.AsMemory(_bufferPosition.._bufferLength);

    /// <summary>
    /// Fill the internal buffer up the desired length. If the buffer's size meets or exceeds the
    /// required length, no async operation is performed and the method exists early.
    /// </summary>
    /// <param name="stream">Stream to consume bytes from to fill the buffer</param>
    /// <param name="length">require length of data in the internal buffer</param>
    /// <param name="cancellationToken">token to cancel the async operation</param>
    /// <exception cref="IOException">if the stream is closed</exception>
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask FillBufferAsync(
        Stream stream,
        int length,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ArrayBufferReader));
        var bytesRemaining = _bufferLength - _bufferPosition;
        if (bytesRemaining >= length)
        {
            return;
        }

        var spaceNeeded = length + bytesRemaining;
        if (spaceNeeded > _readBuffer.Length)
        {
            ReallocateInternalBuffer(spaceNeeded);
        }
        else if (_bufferLength > 0)
        {
            _readBuffer.AsSpan()[_bufferPosition.._bufferLength]
                .CopyTo(_readBuffer);
            _bufferLength = bytesRemaining;
        }

        _bufferPosition = 0;
        var count = length - bytesRemaining;
        while (count > 0)
        {
            var bytesRead = await stream.ReadAsync(
                    _readBuffer.AsMemory(_bufferLength),
                    cancellationToken)
                .ConfigureAwait(false);
            if (bytesRead == 0)
            {
                throw new IOException("Stream closed unexpectedly");
            }

            count -= bytesRead;
            _bufferLength += bytesRead;
        }
    }

    private void ReallocateInternalBuffer(int newSize)
    {
        var tempBuffer = _readBuffer;
        _readBuffer = new byte[newSize];
        tempBuffer.AsSpan()[_bufferPosition.._bufferLength]
            .CopyTo(_readBuffer);
        _bufferLength -= _bufferPosition;
    }

    /// <summary>
    /// Move the buffer's current position forward by the number of bytes consumed by the previous
    /// action.
    /// </summary>
    /// <param name="bytesConsumed">Number of bytes consumed</param>
    public void AdvanceBufferPosition(int bytesConsumed)
    {
        _bufferPosition += bytesConsumed;
    }

    /// <summary>
    /// Reallocate the internal buffer to match the initial capacity. Does nothing if the buffer
    /// size has not changed.
    /// </summary>
    public void ResetToInitialCapacity()
    {
        if (_readBuffer.Length != _initialCapacity) return;

        ReallocateInternalBuffer(_initialCapacity);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _readBuffer = [];
        _bufferLength = 0;
        _bufferPosition = -1;
    }
}
