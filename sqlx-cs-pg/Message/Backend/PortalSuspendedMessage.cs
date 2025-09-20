namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent to signal that the backend has suspended the execution of a portal or cursor and
/// needs to be resumed to continue sending rows. This message is a singleton because the message
/// contains no data.
/// </summary>
internal sealed class PortalSuspendedMessage : IPgBackendMessage
{
    internal static PortalSuspendedMessage Instance { get; } = new(); 
    
    private PortalSuspendedMessage() {}
}
