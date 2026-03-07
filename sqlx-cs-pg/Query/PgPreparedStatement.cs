using System.Globalization;
using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Query;

/// <summary>
/// Postgres representation of a prepared statement
/// </summary>
internal record PgPreparedStatement(string Sql, int StatementId)
{
    public string StatementName => StatementId.ToString(CultureInfo.InvariantCulture);
    public PgColumnMetadata[] ColumnMetadata { get; set; } = [];
}
