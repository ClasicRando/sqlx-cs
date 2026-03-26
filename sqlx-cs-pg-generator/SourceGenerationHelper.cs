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

    public static readonly DiagnosticDescriptor DefinitionShouldBeValueType =
        new(
            "SQLxPG001",
            "Annotated type declaration should be a struct",
            "Currently, '{0}' is a reference type but it's recommended to be a value type",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Warning,
            true);

    public static readonly DiagnosticDescriptor InvalidTypeDefinition =
        new(
            "SQLxPG001",
            "Annotated type declaration is not valid",
            "'{0}' is invalid for the purposes of the attached source generation attribute '{1}'. {2}",
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

    extension(ISymbol symbol)
    {
        public bool HasAttribute(params string[] name) => symbol.GetAttributes()
            .Any(attr => name.Contains(attr.AttributeClass?.Name));
    }

    extension(ITypeSymbol typeSymbol)
    {
        public string FullName
        {
            get
            {
                if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
                {
                    return arrayTypeSymbol.FullName;
                }
                var namespaceFullName = typeSymbol.ContainingNamespace is null
                    ? string.Empty
                    : typeSymbol.ContainingNamespace.GetFullNamespaceName();
                return string.IsNullOrEmpty(namespaceFullName)
                    ? typeSymbol.Name
                    : namespaceFullName + "." + typeSymbol.Name;
            }
        }

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
            typeSymbol.HasAttribute("PgCompositeAttribute", "WrapperTypeAttribute");

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
            return typeSymbol.AsNotNullType().GetDecodeMethodSuffixInner() is not null;
        }

        public string? GetIPgDbType()
        {
            const string typeNamespace = "Sqlx.Postgres.Type";
            string name;
            switch (typeSymbol.AsNotNullType())
            {
                case INamedTypeSymbol { IsDbType: true } namedTypeSymbol:
                    name = namedTypeSymbol.FullName;
                    break;
                case INamedTypeSymbol { IsPgEnum: true } namedTypeSymbol:
                    name = namedTypeSymbol.FullName;
                    break;
                case IArrayTypeSymbol { ElementType.Name: nameof(Byte) }:
                    name = $"{typeNamespace}.PgBytea";
                    break;
                case IArrayTypeSymbol arrayTypeSymbol:
                    var elementType = (INamedTypeSymbol)arrayTypeSymbol.ElementType;
                    var decoderName = elementType.IsValueType ? "PgArrayTypeStruct" : "PgArrayTypeClass";
                    name = $"{typeNamespace}.{decoderName}<{elementType.FullName}, {elementType.GetIPgDbType()}>";
                    break;
                case { Name: nameof(Boolean) }:
                    name = $"{typeNamespace}.PgBool";
                    break;
                case { Name: nameof(SByte) }:
                    name = $"{typeNamespace}.PgChar";
                    break;
                case { Name: nameof(Int16) }:
                    name = $"{typeNamespace}.PgShort";
                    break;
                case { Name: nameof(Int32) }:
                    name = $"{typeNamespace}.PgInt";
                    break;
                case { Name: nameof(Int64) }:
                    name = $"{typeNamespace}.PgLong";
                    break;
                case { Name: nameof(Single) }:
                    name = $"{typeNamespace}.PgFloat";
                    break;
                case { Name: nameof(Double) }:
                    name = $"{typeNamespace}.PgDouble";
                    break;
                case { Name: "TimeOnly" }:
                    name = $"{typeNamespace}.PgTime";
                    break;
                case { Name: "DateOnly" }:
                    name = $"{typeNamespace}.PgDate";
                    break;
                case { Name: nameof(DateTime) }:
                    name = $"{typeNamespace}.PgDateTime";
                    break;
                case { Name: "DateTimeOffset" }:
                    name = $"{typeNamespace}.PgDateTimeOffset";
                    break;
                case { Name: nameof(Decimal) }:
                    name = $"{typeNamespace}.PgDecimal";
                    break;
                case { Name: nameof(String) }:
                    name = $"{typeNamespace}.PgString";
                    break;
                case { Name: nameof(Guid) }:
                    name = $"{typeNamespace}.PgUuid";
                    break;
                case { Name: "IPNetwork" }:
                    name = $"{typeNamespace}.PgIpNetwork";
                    break;
                case { Name: nameof(BitArray) }:
                    name = $"{typeNamespace}.PgBitString";
                    break;
                case INamedTypeSymbol { Name: "PgRange" } namedTypeSymbol:
                    var innerType = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
                    if (!innerType.IsValidRangeType)
                    {
                        return null;
                    }

                    name =
                        $"{typeNamespace}.PgRangeType<{innerType.FullName}, {innerType.GetIPgDbType()}>";
                    break;
                default:
                    return null;
            }

            return name;
        }

        public bool HasIPgDbType()
        {
            return typeSymbol.GetIPgDbType() is not null;
        }
    }

    extension(INamedTypeSymbol namedTypeSymbol)
    {
        private bool IsDecodableEnum => namedTypeSymbol.EnumUnderlyingType is not null &&
                                        namedTypeSymbol.HasAttribute(
                                            "PgEnumAttribute",
                                            "WrapperEnumAttribute");

        private bool IsPgEnum => namedTypeSymbol.EnumUnderlyingType is not null &&
                                 namedTypeSymbol.HasAttribute("PgEnumAttribute");

        private bool IsValidRangeType => namedTypeSymbol.Name is nameof(Int64) or nameof(Int32)
            or "DateOnly" or nameof(DateTime) or "DateTimeOffset" or nameof(Decimal);
    }

    extension(IArrayTypeSymbol arrayTypeSymbol)
    {
        private string FullName => $"{arrayTypeSymbol.ElementType.FullName}[]";
    }

    extension(IPropertySymbol propertySymbol)
    {
        public bool IsNotSkip => !(propertySymbol.IsIndexer ||
                                   propertySymbol.IsImplicitlyDeclared ||
                                   propertySymbol.HasAttribute("PgPropertySkipAttribute"));
    }

    extension(SyntaxNode syntaxNode)
    {
        public bool IsEnum => syntaxNode is EnumDeclarationSyntax;

        public bool IsProductType => syntaxNode is ClassDeclarationSyntax or StructDeclarationSyntax
            or RecordDeclarationSyntax;
    }

    extension(Accessibility accessibility)
    {
        public string GetModifierToken()
        {
            return accessibility switch
            {
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal or Accessibility.Protected => "protected",
                Accessibility.Internal or Accessibility.ProtectedOrInternal => "internal",
                Accessibility.Public => "public",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(accessibility),
                    accessibility,
                    null),
            };
        }
    }
}
