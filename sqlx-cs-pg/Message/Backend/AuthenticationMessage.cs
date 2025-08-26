using Sqlx.Core.Buffer;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;

namespace Sqlx.Postgres.Message.Backend;

internal sealed class AuthenticationMessage(IAuthMessage authMessage) : IPgBackendMessage, IPgBackendMessageDecoder<AuthenticationMessage>
{
    internal IAuthMessage AuthMessage { get; } = authMessage;
    public static AuthenticationMessage Decode(ReadBuffer buffer)
    {
        var authMethod = buffer.ReadInt();
        IAuthMessage message;
        switch (authMethod)
        {
            case 0:
                message = OkAuthMessage.Instance;
                break;
            case 3:
                message = ClearTextPasswordAuthMessage.Instance;
                break;
            case 5:
                message = new MD5PasswordAuthMessage(buffer.ReadBytes(4));
                break;
            case 10:
                List<string> authMechanisms = [];
                while (buffer.Remaining > 0)
                {
                    authMechanisms.Add(buffer.ReadCString());
                }
                message = new SaslAuthMessage(authMechanisms);
                break;
            case 11:
                message = new SaslContinueAuthMessage(buffer.ReadBytes());
                break;
            case 12:
                message = new SaslFinalAuthMessage(buffer.ReadBytes());
                break;
            default:
                throw new PgException($"Unknown authentication method: {authMethod}");
        }

        return new AuthenticationMessage(message);
    }
}
