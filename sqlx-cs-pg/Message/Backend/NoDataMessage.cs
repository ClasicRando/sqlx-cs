namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Message sent by the server to signal that a prepared statement does not return any rows. This
/// message is a singleton because the message contains no data.
/// </summary>
internal sealed class NoDataMessage : IPgBackendMessage
{
    internal static NoDataMessage Instance { get; } = new(); 
    
    private NoDataMessage() {}
}
