using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Stream;

internal partial class PgStream
{
    private async Task SimplePasswordAuthFlow(string username, string password, byte[]? salt = null, CancellationToken cancellationToken = default)
    {
        var passwordBytes = PasswordHelper.CreateSimplePassword(username, password, salt);
        await SendSimplePasswordMessage(passwordBytes, cancellationToken).ConfigureAwait(false);

        var authentication = await ReceiveNextMessageAs<AuthenticationMessage>(cancellationToken)
            .ConfigureAwait(false);
        PgException.CheckIfIs<IAuthMessage, OkAuthMessage>(authentication.AuthMessage);
    }
}
