using Sqlx.Core.Query;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Query;

public interface IPgQueryBatch : IQueryBatch<IPgBindable, IPgDataRow>;
