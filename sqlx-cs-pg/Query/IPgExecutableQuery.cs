using Sqlx.Core.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Query;

public interface IPgExecutableQuery : IExecutableQuery<IPgDataRow>
{
    int ParameterCount { get; }
}
