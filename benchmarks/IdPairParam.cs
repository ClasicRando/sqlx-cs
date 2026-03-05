using Sqlx.Core.Query;
using Sqlx.Postgres.Query;

namespace benchmarks;

public readonly struct IdPairParam : IBindMany<IPgBindable>
{
    public required int Id1 { get; init; }

    public required int Id2 { get; init; }

    public void BindMany(IPgBindable bindable)
    {
        bindable.Bind(Id1);
        bindable.Bind(Id2);
    }
}