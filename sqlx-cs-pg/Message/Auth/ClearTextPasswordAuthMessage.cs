namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Message indicating that clear text password is the authentication method expected. This class is
/// a singleton because the message will never contain any information.
/// </summary>
internal sealed class ClearTextPasswordAuthMessage : IAuthMessage
{
    private ClearTextPasswordAuthMessage() {}

    internal static ClearTextPasswordAuthMessage Instance { get; } = new();
}
