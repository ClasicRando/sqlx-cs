namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Message indicating the final message during SASL authentication flow
/// </summary>
internal sealed class SaslFinalAuthMessage(string saslData) : IAuthMessage
{
    /// <summary>
    /// Data sent from the server containing comma separated key value pairs
    /// </summary>
    internal string SaslData { get; } = saslData;
}
