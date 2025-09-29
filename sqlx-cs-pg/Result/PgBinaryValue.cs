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
    /// <summary>
    /// Slice this binary value for the specified range. This uses a slice of bytes from
    /// <see cref="Buffer"/> as well as copying the <see cref="ColumnMetadata"/> reference. The
    /// underlining <see cref="Buffer"/> is also advanced by the length value so future reads skip
    /// the sliced bytes.
    /// </summary>
    /// <param name="length">Number of bytes to include in the slice</param>
    /// <returns>A sliced subset of this binary value with the same column metadata</returns>
    public PgBinaryValue Slice(int length) => new(Buffer.Slice(length), ref ColumnMetadata);
}
