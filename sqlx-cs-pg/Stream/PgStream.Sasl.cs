using System.Security.Cryptography;
using System.Text;
using Sqlx.Core;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;
using Sqlx.Postgres.Message.Backend;
using Sqlx.Postgres.Message.Frontend;

namespace Sqlx.Postgres.Stream;

internal partial class PgStream
{
    private const string Mechanism = "SCARM-SHA-256";
    private const string CbindFlag = "n";
    private const string Cbind = "biws";
    
    private async Task SaslAuthFlow(
        string password,
        SaslAuthMessage saslAuthMessage,
        CancellationToken cancellationToken)
    {
        var clientNonce = await SendScramInit(saslAuthMessage, cancellationToken)
            .ConfigureAwait(false);
        SaslContinueAuthMessage continueAuthMessage = await ReceiveContinueMessage(cancellationToken)
            .ConfigureAwait(false);
        var serverSignature = await SendClientFinalMessage(
                continueAuthMessage,
                clientNonce,
                password,
                cancellationToken)
            .ConfigureAwait(false);
        SaslFinalAuthMessage finalAuthMessage = await ReceiveFinalAuthMessage(cancellationToken)
            .ConfigureAwait(false);
        var finalServerSignature = ValidateServerFinalMessage(finalAuthMessage);

        if (finalServerSignature != Convert.ToBase64String(serverSignature))
        {
            throw new PgException("Unable to verify server signature");
        }

        await ReceiveOkAuthMessage(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> SendScramInit(SaslAuthMessage saslAuthMessage, CancellationToken cancellationToken)
    {
        var serverSupportsSha256 = saslAuthMessage.AuthMechanisms.Contains("SCARM-SHA-256");
        // var allowSha256 = serverSupportsSha256 &&
        //                   _connectOptions.ChannelBinding != ChannelBinding.Require;
        // var serverSupportsSha256Plus = saslAuthMessage.AuthMechanisms.Contains("SCARM-SHA-256-PLUS");
        // var allowSha256Plus = serverSupportsSha256Plus &&
        //                       _connectOptions.ChannelBinding != ChannelBinding.Disable;

        // if (!allowSha256 && !allowSha256Plus)
        // {
        //     if (serverSupportsSha256 &&
        //         _connectOptions.ChannelBinding == ChannelBinding.Require)
        //     {
        //         throw new PgException("");
        //     }
        //
        //     if (serverSupportsSha256Plus &&
        //         _connectOptions.ChannelBinding == ChannelBinding.Disable)
        //     {
        //         throw new PgException("");
        //     }
        //
        //     throw new PgException("");
        // }

        if (!serverSupportsSha256)
        {
            throw new PgException("Server does not support SCRAM-SHA-256");
        }

        var clientNonce = GetNonce();

        var saslInitialMessage = new SaslInitialMessage(Mechanism, $"{CbindFlag},,n=*,r={clientNonce}");
        await SendMessage(saslInitialMessage, cancellationToken).ConfigureAwait(false);
        return clientNonce;
    }

    private static string GetNonce()
    {
        using var rngProvider = RandomNumberGenerator.Create();
        var nonceBytes = new byte[18];
        
        rngProvider.GetBytes(nonceBytes);
        return Convert.ToBase64String(nonceBytes);
    }

    private async Task<SaslContinueAuthMessage> ReceiveContinueMessage(CancellationToken cancellationToken)
    {
        var authentication = await ReceiveNextMessageAs<AuthenticationMessage>(cancellationToken)
            .ConfigureAwait(false);
        return PgException.CheckIfIs<IAuthMessage, SaslContinueAuthMessage>(
            authentication.AuthMessage);
    }
    
    private async Task<byte[]> SendClientFinalMessage(
        SaslContinueAuthMessage continueAuthMessage,
        string clientNonce,
        string password,
        CancellationToken cancellationToken)
    {
        var (serverNonce, salt, iterations) = ParseSaslContinueData(continueAuthMessage.SaslData);
        if (!serverNonce.StartsWith(clientNonce, StringComparison.Ordinal))
        {
            throw new PgException("Server nonce does not start with client nonce");
        }

        var saltBytes = Convert.FromBase64String(salt);
        var saltedPassword =
            Hi(password.Normalize(NormalizationForm.FormKC), saltBytes, iterations);

        var clientKey = Hmac(saltedPassword, "Client Key");
        var storedKey = SHA256.HashData(clientKey);
        var clientFirstMessageBare = $"n=*,r={clientNonce}";
        var serverFirstMessage = $"r={serverNonce},s={salt},i={iterations}";
        var clientFinalMessageWithoutProof = $"c={Cbind},r={serverNonce}";

        var authMessage = $"{clientFirstMessageBare},{serverFirstMessage},{clientFinalMessageWithoutProof}";

        var clientSignature = Hmac(storedKey, authMessage);
        Xor(clientKey, clientSignature);
        var clientProof = Convert.ToBase64String(clientKey);

        var serverKey = Hmac(saltedPassword, "Server Key");
        var serverSignature = Hmac(serverKey, authMessage);

        var messageStr = $"{clientFinalMessageWithoutProof},r={clientProof}";

        var saslResponse = new SaslResponseMessage(messageStr);
        await SendMessage(saslResponse, cancellationToken).ConfigureAwait(false);
        return serverSignature;
    }

    private static (string Nonce, string Salt, int Iteration) ParseSaslContinueData(
        ReadOnlySpan<byte> saslContinueData)
    {
        var data = Charsets.Default.GetString(saslContinueData);
        string? nonce = null;
        string? salt = null;
        var iteration = -1;

        foreach (var part in data.Split(','))
        {
            if (part.StartsWith("r=", StringComparison.Ordinal))
            {
                nonce = part[2..];
            }
            else if (part.StartsWith("s=", StringComparison.Ordinal))
            {
                salt = part[2..];
            }
            else if (part.StartsWith("i=", StringComparison.Ordinal))
            {
                iteration = int.Parse(part[2..]);
            }
            else
            {
                // Unknown part of SCRAM continue response
            }
        }
        
        PgException.ThrowIfNull(nonce);
        PgException.ThrowIfNull(salt);
        if (iteration == -1)
        {
            throw new PgException("Could not find iteration value within SASL continue message");
        }
        return (nonce, salt, iteration);
    }

    private static byte[] Hi(ReadOnlySpan<char> str, ReadOnlySpan<byte> salt, int count)
        => Rfc2898DeriveBytes.Pbkdf2(str, salt, count, HashAlgorithmName.SHA256, 256 / 8);

    private static void Xor(Span<byte> buf1, Span<byte> buf2)
    {
        for (var i = 0; i < buf1.Length; i++)
        {
            buf1[i] ^= buf2[i];
        }
    }

    private static byte[] Hmac(ReadOnlySpan<byte> key, string data)
        => HMACSHA256.HashData(key, Charsets.Default.GetBytes(data));

    private async Task<SaslFinalAuthMessage> ReceiveFinalAuthMessage(CancellationToken cancellationToken)
    {
        var authentication = await ReceiveNextMessageAs<AuthenticationMessage>(cancellationToken)
            .ConfigureAwait(false);
        return PgException.CheckIfIs<IAuthMessage, SaslFinalAuthMessage>(
            authentication.AuthMessage);
    }

    private static string ValidateServerFinalMessage(SaslFinalAuthMessage finalAuthMessage)
    {
        var data = Charsets.Default.GetString(finalAuthMessage.SaslData);
        string? serverSignature = null;

        foreach (var part in data.Split(','))
        {
            if (part.StartsWith("v=", StringComparison.Ordinal))
            {
                serverSignature = part[2..];
            }
            else
            {
                // Unknown part of SCRAM final response
            }
        }
        
        PgException.ThrowIfNull(serverSignature);
        return serverSignature;
    }

    private async Task ReceiveOkAuthMessage(CancellationToken cancellationToken)
    {
        var authentication = await ReceiveNextMessageAs<AuthenticationMessage>(cancellationToken)
            .ConfigureAwait(false);
        PgException.CheckIfIs<IAuthMessage, OkAuthMessage>(authentication.AuthMessage);
    }
}
