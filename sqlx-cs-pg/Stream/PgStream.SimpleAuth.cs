using Sqlx.Core;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Stream;

public partial class PgStream
{
    /// <summary>
    /// Handle simple password auth flow. This sends the password as a simple message of bytes
    /// optionally MD5 hashed if a salt is specified.
    /// </summary>
    /// <param name="username">Username to include in the MD5 hash (if needed)</param>
    /// <param name="password">Password to encode/hash</param>
    /// <param name="salt">Optional salt if MD5 hash is required</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    private async Task SimplePasswordAuthFlow(
        string username,
        string password,
        byte[]? salt = null,
        CancellationToken cancellationToken = default)
    {
        var isSimplePassword = salt is null;
        var bufferSize = isSimplePassword ? Charsets.Default.GetByteCount(password) : 35;
        var passwordBytes = bufferSize > 256
            ? new byte[bufferSize]
            : stackalloc byte[bufferSize];
        if (isSimplePassword)
        {
            Charsets.Default.GetBytes(password, passwordBytes);
        }
        else
        {
            PasswordHelper.CreateMd5HashedPassword(username, password, salt, passwordBytes);
        }
        
        await SendSimplePasswordMessage(passwordBytes, cancellationToken)
            .ConfigureAwait(false);

        var authentication = await ReceiveNextMessageAs<IAuthMessage>(cancellationToken)
            .ConfigureAwait(false);
        PgException.CheckIfIs<IAuthMessage, OkAuthMessage>(authentication);
    }
}
