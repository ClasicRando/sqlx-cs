using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

/// <summary>
/// Container for text encoded data. Used to deserialize text data into column values.
/// </summary>
public readonly ref struct PgTextValue(
    ReadOnlySpan<char> chars,
    ref PgColumnMetadata columnMetadata)
{
    /// <summary>
    /// Readable span of characters
    /// </summary>
    public readonly ReadOnlySpan<char> Chars = chars;
    /// <summary>
    /// Metadata of the column to read
    /// </summary>
    public readonly ref PgColumnMetadata ColumnMetadata = ref columnMetadata;
    /// <summary>
    /// Slice this text value for the specified range. This copies a slice of characters from
    /// <see cref="Chars"/> as well as copying the <see cref="ColumnMetadata"/> reference.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public PgTextValue Slice(Range range) => new(Chars[range], ref ColumnMetadata);

    public static implicit operator ReadOnlySpan<char>(PgTextValue value) => value.Chars;
}
