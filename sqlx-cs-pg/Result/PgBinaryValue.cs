using Sqlx.Core.Buffer;
using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

public readonly ref struct PgBinaryValue(ReadBuffer buffer, PgColumnMetadata columnMetadata)
{
    public ReadBuffer Buffer { get; } = buffer;

    public PgColumnMetadata ColumnMetadata { get; } = columnMetadata;
}
