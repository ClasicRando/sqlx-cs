using Sqlx.Core.Result;
using Sqlx.Postgres.Result;

namespace Sqlx.Postgres.Type;

public interface IPgTypeKind;

public readonly record struct SimpleType : IPgTypeKind;

public readonly record struct PseudoType : IPgTypeKind;

public readonly record struct DomainType : IPgTypeKind
{
    public required PgTypeInfo InnerType { get; init; }
}

public readonly record struct CompositeType : IPgTypeKind
{
    public readonly record struct Attribute : IFromRow<Attribute>
    {
        public required string Name { get; init; }
        public required PgOid TypeOid { get; init; }

        public static Attribute FromRow(IDataRow dataRow)
        {
            return new Attribute
            {
                Name = dataRow.GetStringNotNull("attname"),
                TypeOid = dataRow.GetPgNotNull<PgOid>("atttypid"),
            };
        }
    }
    
    public required Attribute[] Attributes { get; init; }
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
