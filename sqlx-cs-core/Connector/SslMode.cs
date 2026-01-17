namespace Sqlx.Core.Connector;

public enum SslMode
{
    Disable,
    Allow,
    Prefer,
    Require,
    VerifyCa,
    VerifyFull,
}

public static class SslModeExtensions
{
    extension(SslMode sslMode)
    {
        public bool AcceptInvalidCerts() => sslMode is not (SslMode.VerifyCa or SslMode.VerifyFull);

        public bool AcceptInvalidHostNames() => sslMode is not SslMode.VerifyFull;
    }
}
