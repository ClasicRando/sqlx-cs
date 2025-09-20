namespace Sqlx.Postgres.Message.Auth;

/// <summary>
/// Message indicating that SASL authentication should proceed with the provided SASL data.
/// </summary>
internal sealed class SaslContinueAuthMessage(byte[] saslData) : IAuthMessage
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly byte[] _bytes = saslData;
    /// <summary>
    /// Data sent from the database to continue SASL authentication. This data contains the server's
    /// nonce, salt and number of iterations.
    /// </summary>
    internal ReadOnlySpan<byte> SaslData => _bytes.AsSpan();
}
