namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent to signal that the backend is done parsing a query. This message is a singleton
/// because the message contains no data.
/// </summary>
internal sealed class ParseCompleteMessage : IPgBackendMessage
{
    public static ParseCompleteMessage Instance { get; } = new(); 
    
    private ParseCompleteMessage() {}
}
