namespace Sqlx.Postgres.Message.Auth;

internal sealed class SaslAuthMessage(List<string> authMechanisms) : IAuthMessage
{
    internal IReadOnlyList<string> AuthMechanisms { get; } = authMechanisms;
}
