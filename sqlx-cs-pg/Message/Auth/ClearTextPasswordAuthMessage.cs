namespace Sqlx.Postgres.Message.Auth;

internal sealed class ClearTextPasswordAuthMessage : IAuthMessage
{
    private ClearTextPasswordAuthMessage() {}

    internal static ClearTextPasswordAuthMessage Instance { get; } = new();
}
