using System.Text.Json.Serialization.Metadata;
using Sqlx.Core;
using Sqlx.Core.Query;
using Sqlx.Core.Result;

namespace Sqlx.Postgres.Query;

internal class PgExecutableQuery(string sql, IQueryExecutor queryExecutor) : IExecutableQuery
{
    public string Query { get; } = sql;

    public PgParameterBuffer ParameterBuffer { get; } = new();

    public void Bind<T>(T? value) where T : notnull
    {
        ParameterBuffer.Encode(value);
    }

    public void BindJson<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : notnull
    {
        if (value is null)
        {
            ParameterBuffer.EncodeNull();
            return;
        }
        ParameterBuffer.EncodeJsonValue(value, typeInfo);
    }

    public void BindOutParameter<T>() where T : notnull
    {
        ParameterBuffer.EncodeNull();
    }

    public IAsyncEnumerable<Either<IDataRow, QueryResult>> Execute(CancellationToken cancellationToken)
    {
        return queryExecutor.ExecuteQuery(this, cancellationToken);
    }
}
