namespace Sqlx.Postgres.Message.Backend;

internal sealed class PortalSuspendedMessage : IPgBackendMessage
{
    internal static PortalSuspendedMessage Instance { get; } = new(); 
    
    private PortalSuspendedMessage() {}
}
