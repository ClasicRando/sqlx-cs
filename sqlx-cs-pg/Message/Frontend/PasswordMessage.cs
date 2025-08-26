using System.Security.Cryptography;
using System.Text;
using Sqlx.Core;
using Sqlx.Core.Buffer;
using Sqlx.Postgres.Buffer;

namespace Sqlx.Postgres.Message.Frontend;

internal sealed class PasswordMessage(byte[] password) : IPgFrontendMessage
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly byte[] _password = password;

    public void Encode(WriteBuffer buffer)
    {
        buffer.WriteCode(PgFrontendMessageType.Password);
        buffer.WriteLengthPrefixed(
            true,
            buf =>
            {
                buf.WriteBytes(_password.AsSpan());
                buf.WriteByte(0);
            });
    }
}

internal static class PasswordHelper
{
    internal static PasswordMessage CreateSimplePassword(string username, string password, byte[]? salt)
    {
        return new PasswordMessage(salt is null
            ? Charsets.Default.GetBytes(password)
            : CreateMd5HashedPassword(username, password, salt));
    }

    private static byte[] CreateMd5HashedPassword(string username, string password, byte[] salt)
    {
        var usernameBytes = Charsets.Default.GetBytes(username);
        var passwordBytes = Charsets.Default.GetBytes(password);
        var hexDigest = new byte[35];

        // Initial Digest
        var digestBuffer = new byte[usernameBytes.Length + passwordBytes.Length];
        passwordBytes.CopyTo(digestBuffer.AsSpan());
        usernameBytes.CopyTo(digestBuffer.AsSpan(passwordBytes.Length));
        Md5BytesToHex(MD5.HashData(digestBuffer), hexDigest, 0);

        // Second digest with salt
        digestBuffer = new byte[32 + salt.Length];
        hexDigest.AsSpan(0, 32).CopyTo(digestBuffer.AsSpan());
        salt.CopyTo(digestBuffer.AsSpan(32));
        Md5BytesToHex(MD5.HashData(digestBuffer), hexDigest, 3);

        hexDigest[0] = (byte)'m';
        hexDigest[1] = (byte)'d';
        hexDigest[2] = (byte)'5';
        return hexDigest;
    }

    private static ReadOnlySpan<byte> Lookup => "0123456789abcdef"u8;

    /// <summary>
    /// Convert MD5 bytes into hex character bytes. Pack those hex characters into the
    /// <paramref name="hexDigest"/> buffer.
    /// </summary>
    /// <param name="md5Bytes">bytes returned from an MD5 digest</param>
    /// <param name="hexDigest">output buffer for the resulting hex character bytes</param>
    /// <param name="offset">offset within the <paramref name="hexDigest"/> buffer to start at</param>
    private static void Md5BytesToHex(byte[] md5Bytes, byte[] hexDigest, int offset)
    {
        var pos = offset;
        var i = 0;
        while (i < 16)
        {
            var c = (int)md5Bytes[i];
            var j = c >> 4;
            hexDigest[pos] = Lookup[j];
            pos++;
            j = c & 0xf;
            hexDigest[pos] = Lookup[j];
            pos++;
            i++;
        }
    }
}
