using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

/// <summary>
/// Container for binary encoded data. Used to deserialize binary data into column values.
/// </summary>
public readonly ref struct PgBinaryValue(ReadOnlySpan<byte> buffer, in PgColumnMetadata columnMetadata)
{
    /// <summary>
    /// Readable buffer of binary encoded data
    /// </summary>
    public readonly ReadOnlySpan<byte> Buffer = buffer;

    /// <summary>
    /// Metadata of the column to read
    /// </summary>
    public readonly ref readonly PgColumnMetadata ColumnMetadata = ref columnMetadata;
}
