using Sqlx.Core.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Query;

public interface
    IPgQueryExecutor : IQueryExecutor<IPgExecutableQuery, IPgBindable, IPgQueryBatch, IPgDataRow>;
