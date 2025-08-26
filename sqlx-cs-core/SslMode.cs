namespace Sqlx.Core;

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
    public static bool AcceptInvalidCerts(this SslMode sslMode) => 
        sslMode is not (SslMode.VerifyCa or SslMode.VerifyFull);

    public static bool AcceptInvalidHostNames(this SslMode sslMode) =>
        sslMode is not SslMode.VerifyFull;
}
