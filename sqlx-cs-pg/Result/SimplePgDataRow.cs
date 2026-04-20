namespace Sqlx.Postgres.Result;

internal sealed class SimplePgDataRow : AbstractPgDataRow
{
    public SimplePgDataRow()
    {
    }
    
    public SimplePgDataRow(ReadOnlyMemory<byte> rowData, PgStatementMetadata statementMetadata)
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
