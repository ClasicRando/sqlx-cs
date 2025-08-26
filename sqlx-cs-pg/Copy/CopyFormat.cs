using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Copy;

public enum CopyFormat
{
    Text,
    Csv,
    Binary,
}

public static class CopyFormatExtensions
{
    public static ReadOnlySpan<char> GetName(this CopyFormat copyFormat)
    {
        return copyFormat switch
        {
            CopyFormat.Text => "text",
            CopyFormat.Csv => "csv",
            CopyFormat.Binary => "binary",
            _ => throw new ArgumentOutOfRangeException(nameof(copyFormat), copyFormat, null),
        };
    }

    public static int GetCode(this CopyFormat copyFormat)
    {
        return copyFormat switch
        {
            CopyFormat.Text or CopyFormat.Csv => (int)PgFormatCode.Text,
            CopyFormat.Binary => (int)PgFormatCode.Binary,
            _ => throw new ArgumentOutOfRangeException(nameof(copyFormat), copyFormat, null),
        };
    }

    public static CopyFormat FromByte(byte value)
    {
        return value switch
        {
            0 => CopyFormat.Text,
            1 => CopyFormat.Binary,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}
