using System.Globalization;
using Sqlx.Core.Exceptions;
using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Type;

internal static class HexUtils
{
    /// <summary>
    /// Convert a single char to a digit assuming the character is a hex value. 
    /// </summary>
    /// <param name="chr">Character to parse</param>
    /// <param name="metadata">Column metadata as readonly reference</param>
    /// <typeparam name="T">Type to be decoded</typeparam>
    /// <returns>Parsed integer from the hex char</returns>
    /// <exception cref="ColumnDecodeException">If the character is not valid hex</exception>
    public static int CharToDigit<T>(char chr, in PgColumnMetadata metadata) where T : notnull
    {
        return int.TryParse([chr], NumberStyles.HexNumber, null, out var result)
            ? result
            : throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                metadata,
                $"Character is not valid hex. '{chr}'");
    }

    /// <summary>
    /// Convert a span of characters to a digit assuming the characters are a hex value
    /// </summary>
    /// <param name="chars">Characters to parse</param>
    /// <param name="metadata">Column metadata as readonly reference</param>
    /// <typeparam name="T">Type to be decoded</typeparam>
    /// <returns>Parsed integer from the hex chars</returns>
    /// <exception cref="ColumnDecodeException">If the characters are not valid hex</exception>
    public static int CharsToDigit<T>(
        ReadOnlySpan<char> chars,
        in PgColumnMetadata metadata) where T : notnull
    {
        return int.TryParse(chars, NumberStyles.HexNumber, null, out var result)
            ? result
            : throw ColumnDecodeException.Create<T, PgColumnMetadata>(
                metadata,
                $"Could not decode '{chars}' as a hex number");
    }
}
