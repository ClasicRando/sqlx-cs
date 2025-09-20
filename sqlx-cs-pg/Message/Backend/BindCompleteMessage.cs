namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent as a signal that binding parameters to a prepared statement has been completed.
/// This message is a singleton because the message contains no data.
/// </summary>
internal sealed class BindCompleteMessage : IPgBackendMessage
{
    internal static BindCompleteMessage Instance { get; } = new(); 
    
    private BindCompleteMessage() {}
}
