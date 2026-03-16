using Sqlx.Postgres.Column;

namespace Sqlx.Postgres.Result;

internal sealed class PgStatementMetadata
{
    private readonly PgColumnMetadata[] _columns;
    
    public PgStatementMetadata(PgColumnMetadata[] columns)
    {
        _columns = columns;
    }

    public ref PgColumnMetadata this[int index] => ref _columns[index];

    public int IndexOfFieldName(string name)
    {
        for (var i = 0; i < _columns.Length; i++)
        {
            if (_columns[i].FieldName == name)
            {
                return i;
            }
        }

        return -1;
    }
}
