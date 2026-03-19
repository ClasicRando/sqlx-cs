using Sqlx.Postgres.Generator.Query;

namespace benchmarks;

[ToParam]
public readonly partial struct IdPairParam
{
    public required int Id1 { get; init; }

    public required int Id2 { get; init; }
}