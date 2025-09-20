using Sqlx.Core;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Stream;

internal partial class PgStream
{
    private async Task SimplePasswordAuthFlow(
        string username,
        string password,
        byte[]? salt = null,
        CancellationToken cancellationToken = default)
    {
        var isSimplePassword = salt is null;
        var bufferSize = isSimplePassword ? Charsets.Default.GetByteCount(password) : 35;
        Span<byte> passwordBytes = stackalloc byte[bufferSize];
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
