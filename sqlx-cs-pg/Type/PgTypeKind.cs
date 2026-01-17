using System.Collections.Immutable;
using Sqlx.Core.Result;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

internal interface IPgTypeKind;

public readonly record struct SimpleType : IPgTypeKind;

public readonly record struct PseudoType : IPgTypeKind;

public readonly record struct DomainType : IPgTypeKind
{
    public required PgTypeInfo InnerType { get; init; }
}

public readonly record struct CompositeType : IPgTypeKind
{
    public required ImmutableArray<CompositeField> Fields { get; init; }
}

public readonly record struct CompositeField : IFromRow<IPgDataRow, CompositeField>
{
    public required string Name { get; init; }
    public required PgOid TypeOid { get; init; }

    public static CompositeField FromRow(IPgDataRow dataRow)
    {
        ArgumentNullException.ThrowIfNull(dataRow);
        return new CompositeField
        {
            Name = dataRow.GetStringNotNull("attname"),
            TypeOid = dataRow.GetPgNotNull<PgOid>("atttypid"),
        };
    }
}

public readonly record struct ArrayType : IPgTypeKind
{
    public required PgTypeInfo ElementType { get; init; }
}

public readonly record struct EnumType : IPgTypeKind;

public readonly record struct RangeType : IPgTypeKind
{
    public required PgTypeInfo RangeElement { get; init; }
}

public readonly record struct UnknownType : IPgTypeKind;
