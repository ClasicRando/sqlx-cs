using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

internal class PgStatementMetadata
{
    private readonly Dictionary<string, int> _columnNames = new();
    private readonly PgColumnMetadata[] _columns;
    
    public PgStatementMetadata(PgColumnMetadata[] columns)
    {
        _columns = columns;
        for (var i = 0; i < columns.Length; i++)
        {
            _columnNames[columns[i].FieldName] = i;
        }
    }

    public ref PgColumnMetadata this[int index] => ref _columns[index];

    public int IndexOfFieldName(string name)
    {
        return _columnNames.GetValueOrDefault(name, -1);
    }
}
