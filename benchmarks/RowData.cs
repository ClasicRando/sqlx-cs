using CsvHelper.Configuration.Attributes;
using Sqlx.Core.Query;
using Sqlx.Postgres.Copy;
using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Result;
using Sqlx.Postgres.Query;

namespace benchmarks;

[FromRow(RenameAll = Rename.SnakeCase)]
public readonly partial struct RowData : IPgBinaryCopyRow
{
    public static short ColumnCount => 5;

    [Name("id")]
    public required int Id { get; init; }

    [Name("text_field")]
    [PgName("text_field")]
    public required string Text { get; init; }

    [Name("creation_date")]
    public required DateTime CreationDate { get; init; }

    [Name("last_change_date")]
    public required DateTime LastChangeDate { get; init; }

    [Name("counter")]
    public required int? Counter { get; init; }

    public void BindMany(IPgBindable bindable)
    {
        bindable.Bind(Id);
        bindable.Bind(Text);
        bindable.Bind(CreationDate);
        bindable.Bind(LastChangeDate);
        bindable.Bind(Counter);
    }

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}, {nameof(CreationDate)}: {CreationDate}, {nameof(LastChangeDate)}: {LastChangeDate}, {nameof(Counter)}: {Counter}";
    }
}