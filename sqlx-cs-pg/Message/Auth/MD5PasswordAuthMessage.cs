namespace Sqlx.Postgres.Message.Auth;

// ReSharper disable once InconsistentNaming
internal sealed class MD5PasswordAuthMessage(byte[] bytes) : IAuthMessage
{
    internal byte[] Salt { get; } = bytes;
}
