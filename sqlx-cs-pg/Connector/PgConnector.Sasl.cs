using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Sqlx.Core;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Message.Auth;

namespace Sqlx.Postgres.Connector;

public partial class PgConnector
{
    private const string Mechanism = "SCRAM-SHA-256";
    private const string CbindFlag = "n";
    private const string Cbind = "biws";
    
    /// <summary>
    /// Perform SASL authentication flow. This involves multiple back and forths with the server
    /// to fully authenticate and should resolve with an OK auth message.
    /// </summary>
    /// <param name="password">Password to authenticate the user</param>
    /// <param name="saslAuthMessage">
    /// Initial SASL message sent by the server specifying the auth mechanisms
    /// </param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <exception cref="PgException">
    /// <list type="bullet">
    ///     <item>The server does not support SCRAM-SHA-256</item>
    ///     <item>The received message is not the expected type for the flow</item>
    ///     <item>The server sent nonce does not match to the client nonce</item>
    ///     <item>The server sent data does not contain a required field</item>
    /// </list>
    /// </exception>
    private async Task SaslAuthFlow(
        string password,
        SaslAuthMessage saslAuthMessage,
        CancellationToken cancellationToken)
    {
        var clientNonce = await SendScramInit(saslAuthMessage, cancellationToken)
            .ConfigureAwait(false);
        SaslContinueAuthMessage continueAuthMessage = await ReceiveAuthMessageAs<SaslContinueAuthMessage>(cancellationToken)
            .ConfigureAwait(false);
        var serverSignature = await SendClientFinalMessage(
                continueAuthMessage,
                clientNonce,
                password,
                cancellationToken)
            .ConfigureAwait(false);
        SaslFinalAuthMessage finalAuthMessage = await ReceiveAuthMessageAs<SaslFinalAuthMessage>(cancellationToken)
            .ConfigureAwait(false);
        var finalServerSignature = ValidateServerFinalMessage(finalAuthMessage);

        if (finalServerSignature != Convert.ToBase64String(serverSignature))
        {
            throw new PgException("Unable to verify server signature");
        }

        await ReceiveAuthMessageAs<OkAuthMessage>(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> SendScramInit(
        SaslAuthMessage saslAuthMessage,
        CancellationToken cancellationToken)
    {
        var serverSupportsSha256 = saslAuthMessage.AuthMechanisms.Contains("SCRAM-SHA-256");
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

        await SendSaslInitialMessage(
            Mechanism,
            $"{CbindFlag},,n=*,r={clientNonce}",
            cancellationToken)
            .ConfigureAwait(false);
        return clientNonce;
    }

    /// <summary>
    /// Generate a nonce value as 18 random bytes converted to a Base64 string 
    /// </summary>
    private static string GetNonce()
    {
        using var rngProvider = RandomNumberGenerator.Create();
        var nonceBytes = new byte[18];
        
        rngProvider.GetBytes(nonceBytes);
        return Convert.ToBase64String(nonceBytes);
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

        var clientKey = Hmac(saltedPassword, "Client Key"u8);
        var storedKey = SHA256.HashData(clientKey);
        var clientFirstMessageBare = $"n=*,r={clientNonce}";
        var serverFirstMessage = $"r={serverNonce},s={salt},i={iterations}";
        var clientFinalMessageWithoutProof = $"c={Cbind},r={serverNonce}";

        var authMessage = $"{clientFirstMessageBare},{serverFirstMessage},{clientFinalMessageWithoutProof}";

        var clientSignature = Hmac(storedKey, authMessage);
        Xor(clientKey, clientSignature);
        var clientProof = Convert.ToBase64String(clientKey);

        var serverKey = Hmac(saltedPassword, "Server Key"u8);
        var serverSignature = Hmac(serverKey, authMessage);

        var messageStr = $"{clientFinalMessageWithoutProof},p={clientProof}";

        await SendSaslResponseMessage(messageStr, cancellationToken).ConfigureAwait(false);
        return serverSignature;
    }

    private static (string Nonce, string Salt, int Iteration) ParseSaslContinueData(
        ReadOnlySpan<char> saslContinueData)
    {
        string? nonce = null;
        string? salt = null;
        var iteration = -1;

        foreach (Range range in saslContinueData.Split(','))
        {
            var part = saslContinueData[range];
            if (part.StartsWith("r=", StringComparison.Ordinal))
            {
                nonce = part[2..].ToString();
            }
            else if (part.StartsWith("s=", StringComparison.Ordinal))
            {
                salt = part[2..].ToString();
            }
            else if (part.StartsWith("i=", StringComparison.Ordinal))
            {
                iteration = int.Parse(part[2..], provider: CultureInfo.InvariantCulture);
            }
            else
            {
                // Unknown part of SCRAM continue response
            }
        }
        
        PgException.ThrowIfNull(nonce);
        PgException.ThrowIfNull(salt);
        return iteration == -1
            ? throw new PgException("Could not find iteration value within SASL continue message")
            : (nonce, salt, iteration);
    }

    private static byte[] Hi(ReadOnlySpan<char> str, ReadOnlySpan<byte> salt, int count)
        => Rfc2898DeriveBytes.Pbkdf2(str, salt, count, HashAlgorithmName.SHA256, 256 / 8);

    /// <summary>
    /// XOR the bytes in-place for each index in the first buffer. The second buffer must contain at
    /// least as many bytes as the first buffer.
    /// </summary>
    private static void Xor(Span<byte> buf1, Span<byte> buf2)
    {
        for (var i = 0; i < buf1.Length; i++)
        {
            buf1[i] ^= buf2[i];
        }
    }

    private static byte[] Hmac(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
        => HMACSHA256.HashData(key, data);

    private static byte[] Hmac(ReadOnlySpan<byte> key, string data)
        => Hmac(key, Charsets.Default.GetBytes(data));

    private static string ValidateServerFinalMessage(SaslFinalAuthMessage finalAuthMessage)
    {
        var data = finalAuthMessage.SaslData.AsSpan();
        string? serverSignature = null;

        foreach (Range range in data.Split(','))
        {
            var span = data[range];
            if (span.StartsWith("v=", StringComparison.Ordinal))
            {
                serverSignature = span[2..].ToString();
            }
            else
            {
                // Unknown part of SCRAM final response
            }
        }
        
        PgException.ThrowIfNull(serverSignature);
        return serverSignature;
    }
}
