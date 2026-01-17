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
    extension(CopyFormat copyFormat)
    {
        public ReadOnlySpan<char> GetName =>
            copyFormat switch
            {
                CopyFormat.Text => "text",
                CopyFormat.Csv => "csv",
                CopyFormat.Binary => "binary",
                _ => throw new NotSupportedException($"Invalid copy format '{copyFormat}'"),
            };

        public PgFormatCode GetFormatCode =>
            copyFormat switch
            {
                CopyFormat.Text or CopyFormat.Csv => PgFormatCode.Text,
                CopyFormat.Binary => PgFormatCode.Binary,
                _ => throw new NotSupportedException($"Invalid copy format '{copyFormat}'"),
            };

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
}
