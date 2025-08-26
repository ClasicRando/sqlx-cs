using System.Globalization;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Type;

internal static class HexUtils
{
    public static int CharToDigit<T>(char chr, PgColumnMetadata metadata) where T : notnull
    {
        if (int.TryParse([chr], NumberStyles.HexNumber, null, out var result))
        {
            return result;
        }
        throw ColumnDecodeError.Create<T>(metadata, $"Character is not valid hex. '{chr}'");
    }

    public static int CharsToDigit<T>(
        ReadOnlySpan<char> chars,
        PgColumnMetadata metadata) where T : notnull
    {
        if (int.TryParse(chars, NumberStyles.HexNumber, null, out var result))
        {
            return result;
        }
        throw ColumnDecodeError.Create<T>(
            metadata,
            $"Could not decode '{chars}' as a hex number");
    }
}
