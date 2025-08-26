namespace Sqlx.Core.Stream;

public interface IAsyncStream : IAsyncDisposable
{
    public bool IsConnected { get; }
    
    public Task OpenAsync(string host, ushort port, CancellationToken cancellationToken);
    
    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

    public Task<byte> ReadByteAsync(CancellationToken cancellationToken);

    public Task<int> ReadIntAsync(CancellationToken cancellationToken);

    public ValueTask ReadBuffer(Memory<byte> buffer, CancellationToken cancellationToken);

    public Task CloseAsync(CancellationToken cancellationToken);
}
