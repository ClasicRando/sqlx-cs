namespace Sqlx.Core.Buffer;

/// <summary>
/// Exposes buffered raw byte data for reading. The data is added to the buffer by another component
/// (defined by the concrete implementation) and exposed through the <see cref="Span"/> and
/// <see cref="Memory"/> properties as the entire contents of the buffer. After the desired data is
/// read, call <see cref="AdvanceBufferPosition"/> so the buffer marks those bytes as consumed.
/// Subsequent calls to <see cref="Span"/> or <see cref="Memory"/> will then return buffered bytes
/// after those consumed.
/// </summary>
public interface IBufferReader : IDisposable
{
    /// <summary>
    /// Read-only view of the buffer's content as a <see cref="ReadOnlySpan{T}"/>. This is preferred
    /// over <see cref="Memory"/> because the instance is stack allocated and not leak the view of
    /// the buffered data.
    /// </summary>
    ReadOnlySpan<byte> Span { get; }
    
    /// <summary>
    /// Read-only view of the buffer's content as a <see cref="ReadOnlyMemory{T}"/>. Use this
    /// property when the view must persist across await bound or exist outside of stack memory.
    /// This means you have more flexibility over its usage, but you also might reference a memory
    /// slice that is no longer valid. Prefer using <see cref="Span"/> where possible.
    /// </summary>
    ReadOnlyMemory<byte> Memory { get; }

    /// <summary>
    /// Move the buffer's internal pointer forward by the number of bytes specified. This does not
    /// remove the bytes from the buffer but rather makes them inaccessible from future views of the
    /// buffer contents.
    /// </summary>
    /// <param name="bytesConsumed">Number of bytes consumed from the buffer</param>
    void AdvanceBufferPosition(int bytesConsumed);
}
