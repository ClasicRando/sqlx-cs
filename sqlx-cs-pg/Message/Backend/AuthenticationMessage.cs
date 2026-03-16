using Sqlx.Core.Buffer;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;

namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// <para>
/// <see cref="IPgBackendMessageDecoder{T}"/> for <see cref="IAuthMessage"/> messages. All
/// authentication message contents start with an Int which designates which authentication message
/// type is specified. Depending on the message type, more data might follow.
/// </para>
/// <para>
/// For all authentication messages, search for "Byte1('R')" <a href="https://www.postgresql.org/docs/current/protocol-message-formats.html">here</a>
/// </para> 
/// </summary>
internal abstract class AuthenticationMessage : IPgBackendMessage,
    IPgBackendMessageDecoder<IAuthMessage>
{
    public static PgBackendMessageType MessageType => PgBackendMessageType.Authentication;

    public static IAuthMessage Decode(ReadOnlySpan<byte> buffer)
    {
        var authMethod = buffer.ReadInt();
        switch (authMethod)
        {
            case 0:
                return OkAuthMessage.Instance;
            case 3:
                return ClearTextPasswordAuthMessage.Instance;
            case 5:
                throw new PgException(
                    "MD5 passwords are not supported by sqlx-cs-pg. They have been deprecated " +
                    "for removal by Postgres in version 18 so we will not support their usage");
            case 10:
                List<string> authMechanisms = [];
                while (!buffer.IsEmpty)
                {
                    authMechanisms.Add(buffer.ReadCString());
                }

                return new SaslAuthMessage(authMechanisms);
            case 11:
                return new SaslContinueAuthMessage(buffer.ReadString());
            case 12:
                return new SaslFinalAuthMessage(buffer.ReadString());
            default:
                throw new PgException($"Unknown authentication method: {authMethod}");
        }
    }
}
