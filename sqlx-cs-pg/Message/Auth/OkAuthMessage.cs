namespace Sqlx.Postgres.Message.Auth;

internal sealed class OkAuthMessage : IAuthMessage
{
    private OkAuthMessage() {}

    internal static OkAuthMessage Instance { get; } = new();
}
