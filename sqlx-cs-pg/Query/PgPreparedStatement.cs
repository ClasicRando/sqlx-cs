using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Postgres representation of a prepared statement
/// </summary>
internal record PgPreparedStatement(string Sql, int StatementId)
{
    public string StatementName { get; } = StatementId.ToString();
    public PgColumnMetadata[] ColumnMetadata { get; set; } = [];
}
