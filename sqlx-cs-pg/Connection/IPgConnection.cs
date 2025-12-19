using Sqlx.Core.Connection;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Connection;

public interface IPgConnection :
    IConnection<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>, IPgQueryExecutor;
