namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Message indicating that SASL is the authentication method expected
/// </summary>
internal sealed class SaslAuthMessage(List<string> authMechanisms) : IAuthMessage
{
    /// <summary>
    /// Authentication mechanisms supported under SASL
    /// </summary>
    internal IReadOnlyList<string> AuthMechanisms { get; } = authMechanisms;
}
