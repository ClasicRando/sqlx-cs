using System.Collections;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sqlx.Postgres.Generator;

internal static class SourceGenerationHelper
{
    public static readonly DiagnosticDescriptor DefinitionIsNotPartial =
        new(
            "SQLxPG001",
            "Annotated definition is not partial",
            "'{0}' must be partial to allow for adding implementation details",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    public static readonly DiagnosticDescriptor IntWrapperEnumNotIntBacked =
        new(
            "SQLxPG004",
            "Annotated int wrapper PgEnum is not an int backed enum",
            "'{0}' must be an int backed enum",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    public static readonly DiagnosticDescriptor UnknownDbType =
        new(
            "SQLxPG005",
            "Unknown DB type reference",
            "Type definition {0} must reference a type that can " +
            "be encoded to or decoded from the database. Fields: [{1}]",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    public static readonly DiagnosticDescriptor ExcessiveFieldAttributes =
        new(
            "SQLxPG006",
            "Excessive field attributes",
            "Type definition {0} has multiple row field attributes. Must either " +
            "be Flatten or Json but not both. Field(s): [{1}]",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    extension(INamespaceSymbol namespaceSymbol)
    {
        public string GetFullNamespaceName()
        {
            if (string.IsNullOrEmpty(namespaceSymbol.Name))
            {
                return string.Empty;
            }

            StringBuilder builder = new(namespaceSymbol.Name);
            INamespaceSymbol currentNamespace = namespaceSymbol.ContainingNamespace;
            while (!string.IsNullOrEmpty(currentNamespace.Name))
            {
                builder.Insert(0, '.');
                builder.Insert(0, currentNamespace.Name);
                currentNamespace = currentNamespace.ContainingNamespace;
            }

            return builder.ToString();
        }
    }

    extension(StringBuilder builder)
    {
        public StringBuilder AppendFullName<T>(T fullNameType) where T : IFullNameType
        {
            if (!string.IsNullOrEmpty(fullNameType.ContainingNamespace))
            {
                builder.Append(fullNameType.ContainingNamespace);
                builder.Append('.');
            }

            builder.Append(fullNameType.ShortName);
            return builder;
        }

        public StringBuilder AppendFullName(ITypeSymbol typeSymbol)
        {
            var containingNamespace = typeSymbol.ContainingNamespace.GetFullNamespaceName();
            if (!string.IsNullOrEmpty(containingNamespace))
            {
                builder.Append(containingNamespace);
                builder.Append('.');
            }

            builder.Append(typeSymbol.Name);
            return builder;
        }
    }

    extension(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        public bool IsPartial => typeDeclarationSyntax.Modifiers
            .Any(mod => mod.IsKind(SyntaxKind.PartialKeyword));
    }

    extension(ITypeSymbol typeSymbol)
    {
        public bool IsNullable => typeSymbol.NullableAnnotation is NullableAnnotation.Annotated ||
                                  typeSymbol.Name.StartsWith("Nullable");

        public ITypeSymbol AsNotNullType()
        {
            if (typeSymbol.NullableAnnotation is NullableAnnotation.NotAnnotated)
            {
                return typeSymbol;
            }

            return typeSymbol.Name.StartsWith("Nullable")
                ? ((INamedTypeSymbol)typeSymbol).TypeArguments[0]
                : typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        }

        private bool IsDbType =>
            typeSymbol.AllInterfaces.Any(i => i.Name.StartsWith("IPgDbType")) ||
            typeSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name is "PgCompositeAttribute");

        public string? GetDecodeMethodSuffix()
        {
            var isNullable = typeSymbol.IsNullable;
            ITypeSymbol finalType = typeSymbol.AsNotNullType();
            var name = finalType.GetDecodeMethodSuffixInner();
            if (name is "PgVal" or "PgRef")
            {
                return isNullable ? name : "PgNotNull";
            }

            return isNullable ? name : name + "NotNull";
        }

        private string? GetDecodeMethodSuffixInner()
        {
            return typeSymbol switch
            {
                { IsDbType: true } => typeSymbol.IsValueType ? "PgVal" : "PgRef",
                INamedTypeSymbol { IsDecodableEnum: true } => typeSymbol.Name,
                IArrayTypeSymbol { ElementType.Name: nameof(Byte) } => "Bytes",
                IArrayTypeSymbol arrayTypeSymbol =>
                    $"{arrayTypeSymbol.ElementType.AsNotNullType().GetDecodeMethodSuffixInner()}Array",
                { Name: nameof(Boolean) } => "Boolean",
                { Name: nameof(SByte) } => "Byte",
                { Name: nameof(Int16) } => "Short",
                { Name: nameof(Int32) } => "Int",
                { Name: nameof(Int64) } => "Long",
                { Name: nameof(Single) } => "Float",
                { Name: nameof(Double) } => "Double",
                { Name: "TimeOnly" } => "Time",
                { Name: "DateOnly" } => "Date",
                { Name: nameof(DateTime) } => "DateTime",
                { Name: "DateTimeOffset" } => "DateTimeOffset",
                { Name: nameof(Decimal) } => "Decimal",
                { Name: nameof(String) } => "String",
                { Name: nameof(Guid) } => "Guid",
                { Name: "IPNetwork" } => "IpNetwork",
                { Name: nameof(BitArray) } => "BitArray",
                INamedTypeSymbol { Name: "PgRange" } namedTypeSymbol => namedTypeSymbol
                        .TypeArguments[0] switch
                    {
                        { Name: nameof(Int64) } => "PgRangeLong",
                        { Name: nameof(Int32) } => "PgRangeInt",
                        { Name: "DateOnly" } => "PgRangeDate",
                        { Name: nameof(DateTime) } => "PgRangeDateTime",
                        { Name: "DateTimeOffset" } => "PgRangeDateTimeOffset",
                        { Name: nameof(Decimal) } => "PgRangeDecimal",
                        _ => null,
                    },
                _ => null,
            };
        }

        public bool IsValidDbType()
        {
            return typeSymbol.GetDecodeMethodSuffixInner() is not null;
        }
    }

    extension(INamedTypeSymbol namedTypeSymbol)
    {
        private bool IsDecodableEnum => namedTypeSymbol.EnumUnderlyingType is not null &&
                                        namedTypeSymbol.GetAttributes().Any(attr =>
                                            attr.AttributeClass?.Name is "PgEnumAttribute"
                                                or "WrapperEnumAttribute");
    }
}
