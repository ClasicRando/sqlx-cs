namespace Sqlx.Postgres.Type;

public interface IPgTypeKind;

public readonly record struct SimpleType : IPgTypeKind;

public readonly record struct PseudoType : IPgTypeKind;

public readonly record struct DomainType(PgTypeInfo InnerType) : IPgTypeKind;

public readonly record struct CompositeType : IPgTypeKind;

public readonly record struct ArrayType(PgTypeInfo ElementType) : IPgTypeKind;

public readonly record struct EnumType : IPgTypeKind;

public readonly record struct RangeType(PgTypeInfo RangeElement) : IPgTypeKind;

public readonly record struct UnknownType : IPgTypeKind;
