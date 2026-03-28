using System.Buffers;
using CsvHelper.Configuration.Attributes;
using Sqlx.Core.Result;
using Sqlx.Postgres.Generator;
using Sqlx.Postgres.Generator.Copy;
using Sqlx.Postgres.Result;

namespace benchmarks;

// [FromRow(RenameAll = Rename.SnakeCase), ToPgBinaryCopyRow]
[ToPgBinaryCopyRow]
public readonly partial struct RowData : IFromRow<IPgDataRow, RowData>
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
    
    public static RowData FromRow(IPgDataRow dataRow)
    {
        return new RowData
        {
            Id = dataRow.GetField<int>("id"),
            Text = dataRow.GetField<string>("text_field"),
            CreationDate = dataRow.GetField<DateTime>("creation_date"),
            LastChangeDate = dataRow.GetField<DateTime>("last_change_date"),
            Counter = dataRow.GetField<int?>("counter"),
        };
    }

    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(Text)}: {Text}, {nameof(CreationDate)}: {CreationDate}, {nameof(LastChangeDate)}: {LastChangeDate}, {nameof(Counter)}: {Counter}";
    }
}