using BenchmarkDotNet.Running;
using Sqlx.Core.Query;
using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Query;

namespace benchmarks;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}

[FromRow(RenameAll = Rename.SnakeCase)]
public readonly partial struct RowData
{
    public required int Id { get; init; }
        
    [PgName("text_field")]
    public required string Text { get; init; }
        
    public required DateTime CreationDate { get; init; }
        
    public required DateTime LastChangeDate { get; init; }
        
    public required int? Counter { get; init; }

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}, {nameof(CreationDate)}: {CreationDate}, {nameof(LastChangeDate)}: {LastChangeDate}, {nameof(Counter)}: {Counter}";
    }
}

public readonly struct IdParam : IBindMany<IPgBindable>
{
    public required int Id { get; init; }

    public void BindMany(IPgBindable bindable)
    {
        bindable.Bind(Id);
    }
}

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
