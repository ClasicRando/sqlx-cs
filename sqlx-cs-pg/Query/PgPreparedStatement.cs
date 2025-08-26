using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Query;

internal class PgPreparedStatement(string sql, int statementId)
{
    public string Sql { get; } = sql;
    public int StatementId { get; } = statementId;
    public string StatementName { get;  } = statementId.ToString();
    public PgColumnMetadata[] ColumnMetadata { get; set; } = [];
}
