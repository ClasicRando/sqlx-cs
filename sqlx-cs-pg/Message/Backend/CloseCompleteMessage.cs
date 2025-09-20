namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent to signal that a previous close command completed. This message is a singleton
/// because the message contains no data.
/// </summary>
internal sealed class CloseCompleteMessage : IPgBackendMessage
{
    internal static CloseCompleteMessage Instance { get; } = new(); 
    
    private CloseCompleteMessage() {}
}
