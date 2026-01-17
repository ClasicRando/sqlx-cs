using Sqlx.Core;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Connector;

public partial class PgConnector
{
    /// <summary>
    /// Handle simple password auth flow. This sends the password as a simple message of bytes.
    /// </summary>
    /// <param name="password">Password to encode/hash</param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    private async Task SimplePasswordAuthFlow(
        string password,
        CancellationToken cancellationToken = default)
    {
        var bufferSize = Charsets.Default.GetByteCount(password);
        var passwordBytes = bufferSize > 256
            ? new byte[bufferSize]
            : stackalloc byte[bufferSize];
        Charsets.Default.GetBytes(password, passwordBytes);
        
        await SendSimplePasswordMessage(passwordBytes, cancellationToken)
            .ConfigureAwait(false);

        var authentication = await ReceiveNextMessageAs<IAuthMessage>(cancellationToken)
            .ConfigureAwait(false);
        PgException.CheckIfIs<IAuthMessage, OkAuthMessage>(authentication);
    }
}
