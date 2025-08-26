namespace Sqlx.Postgres.Message.Backend;

internal sealed class CloseCompleteMessage : IPgBackendMessage
{
    internal static CloseCompleteMessage Instance { get; } = new(); 
    
    private CloseCompleteMessage() {}
}
