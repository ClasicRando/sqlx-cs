using Sqlx.Core.Query;
using Sqlx.Postgres.Query;

namespace benchmarks;

public readonly struct IdParam : IBindMany<IPgBindable>
{
    public required int Id { get; init; }

    public void BindMany(IPgBindable bindable)
    {
        bindable.Bind(Id);
    }
}