namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Message indicating that the authentication flow succeed and the database will now allow for
/// querying. This class is a singleton because the message will never contain any information.
/// </summary>
internal sealed class OkAuthMessage : IAuthMessage
{
    private OkAuthMessage()
    {
    }

    internal static OkAuthMessage Instance { get; } = new();
}
