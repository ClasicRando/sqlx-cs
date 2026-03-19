using Sqlx.Postgres.Generator.Query;

namespace benchmarks;

[ToParam]
public readonly partial struct IdParam
{
    public required int Id { get; init; }
}