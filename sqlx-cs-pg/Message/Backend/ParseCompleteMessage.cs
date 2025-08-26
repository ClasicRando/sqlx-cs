namespace Sqlx.Postgres.Message.Backend;

internal sealed class ParseCompleteMessage : IPgBackendMessage
{
    public static ParseCompleteMessage Instance { get; } = new(); 
    
    private ParseCompleteMessage() {}
}
