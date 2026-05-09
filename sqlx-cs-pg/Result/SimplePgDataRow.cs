using System.Text.Json;

namespace Sqlx.Postgres.Result;

internal sealed class SimplePgDataRow : AbstractPgDataRow
{
    public SimplePgDataRow(JsonSerializerOptions jsonSerializerOptions)
        : base(jsonSerializerOptions)
    {
    }

    public SimplePgDataRow(
        ReadOnlyMemory<byte> rowData,
        PgStatementMetadata statementMetadata,
        JsonSerializerOptions jsonSerializerOptions) : base(jsonSerializerOptions)
    {
        RowData = rowData;
        StatementMetadata = statementMetadata;
    }


    public void SetRowData(ReadOnlyMemory<byte> rowData, PgStatementMetadata statementMetadata)
    {
        RowData = rowData;
        StatementMetadata = statementMetadata;
    }

    protected override void Dispose(bool disposing)
    {
    }
}
