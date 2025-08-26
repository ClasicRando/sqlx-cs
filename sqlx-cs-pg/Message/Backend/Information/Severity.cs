namespace Sqlx.Postgres.Message.Backend.Information;

public enum Severity
{
    Error,
    Fatal,
    Panic,
    Warning,
    Notice,
    Debug,
    Info,
    Log,
}

public static class SeverityExtensions
{
    private const string Error = "ERROR";
    private const string Fatal = "FATAL";
    private const string Panic = "PANIC";
    private const string Warning = "WARNING";
    private const string Notice = "NOTICE";
    private const string Debug = "DEBUG";
    private const string Info = "INFO";
    private const string Log = "LOG";

    public static Severity FromString(string value)
    {
        return value switch
        {
            Error => Severity.Error,
            Fatal => Severity.Fatal,
            Panic => Severity.Panic,
            Warning => Severity.Warning,
            Notice => Severity.Notice,
            Debug => Severity.Debug,
            Info => Severity.Info,
            Log => Severity.Log,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, ""),
        };
    }

    public static ReadOnlySpan<char> AsReadOnlySpan(this Severity severity)
    {
        return severity switch
        {
            Severity.Error => Error,
            Severity.Fatal => Fatal,
            Severity.Panic => Panic,
            Severity.Warning => Warning,
            Severity.Notice => Notice,
            Severity.Debug => Debug,
            Severity.Info => Info,
            Severity.Log => Log,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, ""),
        };
    }
}
