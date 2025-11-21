namespace Sqlx.Postgres.Type;

public interface IPgTypeKind;

public readonly record struct SimpleType : IPgTypeKind;

public readonly record struct PseudoType : IPgTypeKind;

public readonly record struct DomainType(PgTypeInfo InnerType) : IPgTypeKind;

public readonly record struct CompositeType(KeyValuePair<string, PgTypeInfo> Attributes) : IPgTypeKind;

public readonly record struct ArrayType(PgTypeInfo ElementType) : IPgTypeKind;

public readonly record struct EnumType(string[] Labels) : IPgTypeKind;

public readonly record struct RangeType(PgTypeInfo RangeElement) : IPgTypeKind;

public readonly record struct UnknownType : IPgTypeKind;
