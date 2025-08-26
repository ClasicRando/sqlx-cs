namespace Sqlx.Postgres.Message.Backend;

internal sealed class CopyDoneMessage : IPgBackendMessage
{
    internal static CopyDoneMessage Instance { get; } = new(); 
    
    private CopyDoneMessage() {}
}
