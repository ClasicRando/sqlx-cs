using System.Security.Cryptography;
using Sqlx.Core;

namespace Sqlx.Postgres.Message.Frontend;

/// <summary>
/// Helper class for handling password message creation as either simple and MD5 hashed passwords.
/// </summary>
internal static class PasswordHelper
{
    /// <summary>
    /// Perform an MD5 hash digest on the username, password and salt. Resulting bytes are written
    /// to the final <paramref name="hexDigest"/> parameter.
    /// </summary>
    /// <param name="username">Username used for authentication</param>
    /// <param name="password">Password used for authentication</param>
    /// <param name="salt">Salt sent by the server for MD5 hashing</param>
    /// <param name="hexDigest">Output buffer to write final digest bytes to</param>
    public static void CreateMd5HashedPassword(
        ReadOnlySpan<char> username,
        ReadOnlySpan<char> password,
        ReadOnlySpan<byte> salt,
        Span<byte> hexDigest)
    {
        var usernameByteCount = Charsets.Default.GetByteCount(username);
        var passwordByteCount = Charsets.Default.GetByteCount(password);

        // Initial Digest
        var digestBufferSize = usernameByteCount + passwordByteCount;
        var digestBuffer = digestBufferSize < 255
            ? stackalloc byte[digestBufferSize]
            : new byte[digestBufferSize];
        Charsets.Default.GetBytes(password, digestBuffer[..passwordByteCount]);
        Charsets.Default.GetBytes(username, digestBuffer[passwordByteCount..]);
        Md5BytesToHex(MD5.HashData(digestBuffer), hexDigest, 0);

        // Second digest with salt (salt length is always 4)
        digestBuffer = stackalloc byte[32 + 4];
        hexDigest[..32].CopyTo(digestBuffer);
        salt.CopyTo(digestBuffer[32..]);
        Md5BytesToHex(MD5.HashData(digestBuffer), hexDigest, 3);

        hexDigest[0] = (byte)'m';
        hexDigest[1] = (byte)'d';
        hexDigest[2] = (byte)'5';
    }

    private static ReadOnlySpan<byte> Lookup => "0123456789abcdef"u8;

    /// <summary>
    /// Convert MD5 bytes into hex character bytes. Pack those hex characters into the
    /// <paramref name="hexDigest"/> buffer.
    /// </summary>
    /// <param name="md5Bytes">bytes returned from an MD5 digest</param>
    /// <param name="hexDigest">output buffer for the resulting hex character bytes</param>
    /// <param name="offset">offset within the <paramref name="hexDigest"/> buffer to start at</param>
    private static void Md5BytesToHex(Span<byte> md5Bytes, Span<byte> hexDigest, int offset)
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
