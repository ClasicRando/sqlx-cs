namespace Sqlx.Postgres.Message.Backend;

internal sealed class BindCompleteMessage : IPgBackendMessage
{
    internal static BindCompleteMessage Instance { get; } = new(); 
    
    private BindCompleteMessage() {}
}
