using CsvHelper.Configuration.Attributes;
using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Copy;
using Sqlx.Postgres.Generator.Result;

namespace benchmarks;

[FromRow(RenameAll = Rename.SnakeCase), ToPgBinaryCopyRow]
public readonly partial struct RowData
{
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

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}, {nameof(CreationDate)}: {CreationDate}, {nameof(LastChangeDate)}: {LastChangeDate}, {nameof(Counter)}: {Counter}";
    }
}