namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Backend message indicating that MD5 password hashing is the authentication method expected.
/// </summary>
// ReSharper disable once InconsistentNaming
internal sealed class MD5PasswordAuthMessage(byte[] bytes) : IAuthMessage
{
    /// <summary>
    /// Salt to be used for MD5 hashing
    /// </summary>
    internal byte[] Salt { get; } = bytes;
}
