using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

public readonly ref struct PgTextValue(ReadOnlySpan<char> chars, PgColumnMetadata columnMetadata)
{
    public ReadOnlySpan<char> Chars { get; } = chars;

    public PgColumnMetadata ColumnMetadata { get; } = columnMetadata;

    public PgTextValue Slice(Range range) => new(Chars[range], ColumnMetadata);

    public static implicit operator ReadOnlySpan<char>(PgTextValue value) => value.Chars;
}
