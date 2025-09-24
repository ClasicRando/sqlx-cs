using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

/// <summary>
/// Container for binary encoded data. Used to deserialize binary data into column values.
/// </summary>
public ref struct PgBinaryValue(ReadBuffer buffer, ref PgColumnMetadata columnMetadata)
{
    /// <summary>
    /// Readable buffer of binary encoded data
    /// </summary>
    public ReadBuffer Buffer = buffer;
    /// <summary>
    /// Metadata of the column to read
    /// </summary>
    public readonly ref PgColumnMetadata ColumnMetadata = ref columnMetadata;
}
