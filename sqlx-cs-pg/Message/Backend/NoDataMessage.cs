namespace Sqlx.Postgres.Message.Backend;

internal sealed class NoDataMessage : IPgBackendMessage
{
    internal static NoDataMessage Instance { get; } = new(); 
    
    private NoDataMessage() {}
}
