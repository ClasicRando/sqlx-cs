namespace Sqlx.Postgres.Message.Auth;

internal sealed class SaslContinueAuthMessage(byte[] saslData) : IAuthMessage
{
    // ReSharper disable once ReplaceWithPrimaryConstructorParameter
    private readonly byte[] _bytes = saslData;
    internal ReadOnlySpan<byte> SaslData => _bytes.AsSpan();
}
