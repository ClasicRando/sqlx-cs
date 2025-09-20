namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent to signal that a <c>COPY TO</c> operation is done and the client should stop
/// processing CopyServerData messages from the backend. This message is a singleton because the
/// message contains no data.
/// </summary>
internal sealed class CopyDoneMessage : IPgBackendMessage
{
    internal static CopyDoneMessage Instance { get; } = new(); 
    
    private CopyDoneMessage() {}
}
