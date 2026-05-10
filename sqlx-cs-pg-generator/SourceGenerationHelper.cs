using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
            "Type reference must be a type that can be encoded to or decoded " +
            "from the database. {0}.",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    public static readonly DiagnosticDescriptor ExcessiveFieldAttributes =
        new(
            "SQLxPG006",
            "Excessive field attributes",
            "Type definition {0} has multiple row field attributes. Must either " +
            "be Flatten or Json but not both. Field(s): [{1}].",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    public static readonly DiagnosticDescriptor DefinitionShouldBeValueType =
        new(
            "SQLxPG002",
            "Annotated type declaration should be a struct",
            "Currently, '{0}' is a reference type but it's recommended to be a value type",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Warning,
            true);

    public static readonly DiagnosticDescriptor InvalidTypeDefinition =
        new(
            "SQLxPG003",
            "Annotated type declaration is not valid",
            "'{0}' is invalid for the purposes of the attached source generation attribute '{1}'. {2}.",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);

    private static readonly ConcurrentDictionary<INamespaceSymbol, string> FullNamespaceNameLookup =
        new(SymbolEqualityComparer.Default);

    private static readonly ConcurrentDictionary<ITypeSymbol, string> FullNameLookup =
        new(SymbolEqualityComparer.IncludeNullability);

    private static void AddStringBuilderSliceToLookup(
        StringBuilder builder,
        int startIndex,
        ITypeSymbol typeSymbol)
    {
        var length = builder.Length - startIndex;
        var buffer = new char[length];
        builder.CopyTo(startIndex, buffer.AsSpan(), length);
        FullNameLookup[typeSymbol] = new string(buffer);
    }

    extension(INamespaceSymbol namespaceSymbol)
    {
        public string FullName =>
            FullNamespaceNameLookup.TryGetValue(namespaceSymbol, out var value)
                ? value
                : new StringBuilder()
                    .AppendFullNamespace(namespaceSymbol, includeTrailingSeparator: false)
                    .ToString();

        private IEnumerable<string> GetNamespaceComponents()
        {
            yield return namespaceSymbol.Name;
            INamespaceSymbol currentNamespace = namespaceSymbol.ContainingNamespace;
            while (!string.IsNullOrEmpty(currentNamespace.Name))
            {
                yield return currentNamespace.Name;
                currentNamespace = currentNamespace.ContainingNamespace;
            }
        }
    }

    extension(StringBuilder builder)
    {
        private StringBuilder AppendFullNamespace(
            INamespaceSymbol namespaceSymbol,
            bool includeTrailingSeparator = true)
        {
            if (string.IsNullOrEmpty(namespaceSymbol.Name))
            {
                return builder;
            }

            if (FullNamespaceNameLookup.TryGetValue(namespaceSymbol, out var fullName))
            {
                builder.Append(fullName);
                if (includeTrailingSeparator)
                {
                    builder.Append('.');
                }

                return builder;
            }

            var startIndex = builder.Length;
            foreach (var component in namespaceSymbol.GetNamespaceComponents().Reverse())
            {
                builder.Append(component);
                builder.Append('.');
            }

            var length = builder.Length - 1 - startIndex;
            var buffer = new char[length];
            builder.CopyTo(startIndex, buffer.AsSpan(), length);
            FullNamespaceNameLookup[namespaceSymbol] = new string(buffer);

            if (!includeTrailingSeparator)
            {
                builder.Remove(builder.Length - 1, 1);
            }

            return builder;
        }

        public StringBuilder AppendNamespaceDeclaration(INamespaceSymbol namespaceSymbol)
        {
            var fullName = namespaceSymbol.FullName;
            if (string.IsNullOrEmpty(fullName)) return builder;

            builder.AppendLine();
            return builder.Append("namespace ")
                .Append(fullName)
                .AppendLine(";");
        }

        public StringBuilder AppendFullName<T>(T fullNameType) where T : IFullNameType
        {
            return builder.Append("global::")
                .AppendFullNamespace(fullNameType.ContainingNamespace)
                .Append(fullNameType.ShortName);
        }

        public StringBuilder AppendFullName(ITypeSymbol typeSymbol)
        {
            if (FullNameLookup.TryGetValue(typeSymbol, out var fullName))
            {
                return builder.Append(fullName);
            }

            var startIndex = builder.Length;
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                builder.AppendFullName(arrayTypeSymbol);
                AddStringBuilderSliceToLookup(builder, startIndex, typeSymbol);
                return builder;
            }

            builder.Append("global::")
                .AppendFullNamespace(typeSymbol.ContainingNamespace)
                .Append(typeSymbol.Name);

            if (typeSymbol is { IsReferenceType: true, IsNullable: true })
            {
                builder.Append('?');
            }

            if (typeSymbol is not INamedTypeSymbol { TypeArguments.IsEmpty: false } namedTypeSymbol)
            {
                AddStringBuilderSliceToLookup(builder, startIndex, typeSymbol);
                return builder;
            }

            builder.Append('<');
            for (var i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
            {
                builder.AppendFullName(namedTypeSymbol.TypeArguments[i]);
                if (i != namedTypeSymbol.TypeArguments.Length - 1)
                {
                    builder.Append(',');
                }
            }

            builder.Append('>');
            AddStringBuilderSliceToLookup(builder, startIndex, typeSymbol);
            return builder;
        }

        private StringBuilder AppendFullName(IArrayTypeSymbol typeSymbol)
        {
            return builder.AppendFullName(typeSymbol.ElementType)
                .Append("[]");
        }
    }

    extension(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        public bool IsPartial => typeDeclarationSyntax.Modifiers
            .Any(mod => mod.IsKind(SyntaxKind.PartialKeyword));
    }

    extension(ISymbol symbol)
    {
        private bool HasAttribute(params string[] name) => symbol.GetAttributes()
            .Any(attr => name.Contains(attr.AttributeClass?.Name));
    }

    extension<T>(T typeSymbol) where T : ITypeSymbol
    {
        public T AsNotNullType()
        {
            if (typeSymbol.NullableAnnotation is NullableAnnotation.NotAnnotated)
            {
                return typeSymbol;
            }

            ITypeSymbol newType = typeSymbol.Name.StartsWith("Nullable")
                ? ((INamedTypeSymbol)typeSymbol).TypeArguments[0]
                : typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            return newType is T result
                ? result
                : throw new InvalidOperationException(
                    $"Cannot get non-null variant of type because it's not a {typeof(T)}");
        }
    }

    private static readonly ConcurrentDictionary<ITypeSymbol, string?> PgDbTypeLookup =
        new(SymbolEqualityComparer.Default);

    extension(ITypeSymbol typeSymbol)
    {
        public string FullName =>
            FullNameLookup.TryGetValue(typeSymbol, out var value)
                ? value
                : new StringBuilder().AppendFullName(typeSymbol).ToString();

        public bool IsSystemJsonType
        {
            get
            {
                var fullName = typeSymbol.FullName;
                return fullName is "global::System.Text.Json.JsonElement"
                    or "global::System.Text.Json.JsonDocument"
                    or "global::System.Text.Json.Nodes.JsonNode"
                    or "global::System.Text.Json.Nodes.JsonArray"
                    or "global::System.Text.Json.Nodes.JsonObject"
                    or "global::System.Text.Json.Nodes.JsonValue";
            }
        }

        public bool IsNullable => typeSymbol.NullableAnnotation is NullableAnnotation.Annotated ||
                                  typeSymbol.Name.StartsWith("Nullable");

        public bool IsDbType =>
            typeSymbol.AllInterfaces.Any(i => i.Name.StartsWith("IPgDbType")) ||
            typeSymbol.HasAttribute("PgCompositeAttribute", "WrapperTypeAttribute");

        public bool IsWrapperJson([NotNullWhen(true)] out ITypeSymbol? innerType)
        {
            if (typeSymbol is INamedTypeSymbol
                {
                    Name: "JsonValue",
                    ContainingNamespace: var containingNamespace,
                    TypeArguments: var typeArguments,
                } &&
                containingNamespace.FullName == "Sqlx.Core.Types" &&
                typeArguments.Length == 1)
            {
                innerType = typeArguments[0];
                return true;
            }

            innerType = null;
            return false;
        }

        public string? GetIPgDbType()
        {
            const string typeNamespace = "global::Sqlx.Postgres.Type";
            if (PgDbTypeLookup.TryGetValue(typeSymbol, out var value))
            {
                return value;
            }

            ITypeSymbol nonNullType = typeSymbol.AsNotNullType();
            string? name;
            switch (nonNullType)
            {
                case INamedTypeSymbol { IsDbType: true } namedTypeSymbol:
                    name = namedTypeSymbol.FullName;
                    break;
                case INamedTypeSymbol { IsPgEnum: true } namedTypeSymbol:
                    name = $"global::Sqlx.Postgres.Generator.Type.Pg{namedTypeSymbol.Name}";
                    break;
                case IArrayTypeSymbol { ElementType.Name: nameof(Byte) }:
                    name = $"{typeNamespace}.PgBytea";
                    break;
                case IArrayTypeSymbol arrayTypeSymbol:
                    var elementType = (INamedTypeSymbol)arrayTypeSymbol.ElementType;
                    var decoderName = elementType.IsValueType
                        ? "PgArrayTypeStruct"
                        : "PgArrayTypeClass";
                    var dbType = elementType.GetIPgDbType();
                    if (dbType is null)
                    {
                        return null;
                    }

                    name =
                        $"{typeNamespace}.{decoderName}<{elementType.AsNotNullType().FullName}, {dbType}>";
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
                case { IsSystemJsonType: true }:
                    name = $"{typeNamespace}.PgJson<{typeSymbol.FullName}>";
                    break;
                case INamedTypeSymbol { IsRangeType: true } namedTypeSymbol:
                    var innerType = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
                    if (!innerType.IsValidRangeType)
                    {
                        return null;
                    }

                    name =
                        $"{typeNamespace}.PgRangeType<{innerType.FullName}, {innerType.GetIPgDbType()}>";
                    break;
                default:
                    name = nonNullType.IsWrapperJson(out ITypeSymbol? innerFullName)
                        ? $"{typeNamespace}.PgJson<{innerFullName.FullName}>"
                        : null;
                    break;
            }

            PgDbTypeLookup[typeSymbol] = name;
            return name;
        }

        public bool HasIPgDbType()
        {
            return typeSymbol.GetIPgDbType() is not null ||
                   typeSymbol is INamedTypeSymbol { IsWrapperEnum: true };
        }
    }

    extension(INamedTypeSymbol namedTypeSymbol)
    {
        public bool IsWrapperEnum => namedTypeSymbol.EnumUnderlyingType is not null &&
                                     namedTypeSymbol.HasAttribute("WrapperEnumAttribute");

        public bool IsPgEnum => namedTypeSymbol.EnumUnderlyingType is not null &&
                                namedTypeSymbol.HasAttribute("PgEnumAttribute");

        public bool IsRangeType => namedTypeSymbol.Name == "PgRange";

        private bool IsValidRangeType => namedTypeSymbol.Name is nameof(Int64) or nameof(Int32)
            or "DateOnly" or nameof(DateTime) or "DateTimeOffset" or nameof(Decimal);

        public ImmutableArray<KeyValuePair<string, string>> GenerateFieldLookup(Rename renameAll)
        {
            return
            [
                ..namedTypeSymbol.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Select(field =>
                    {
                        var name = field.Name;
                        var overrideName = (string?)field.GetAttributes()
                            .FirstOrDefault(a => a.AttributeClass?.Name == "PgNameAttribute")
                            ?.ConstructorArguments
                            .FirstOrDefault()
                            .Value;
                        var value = overrideName ?? renameAll.TransformName(name);
                        return new KeyValuePair<string, string>(name, value.Replace("\"", "\\\""));
                    }),
            ];
        }
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

    public static StringBuilder CreateInitialSourceGeneratedFileBuilder()
    {
        StringBuilder builder = new(
            """
            // <auto-generated/>
            #nullable enable
            """);
        return builder.AppendLine();
    }

    public static string GetSourceInterceptorFileName(
        string interceptorTargetType,
        ITypeSymbol nonNullType,
        bool isNullableType)
    {
        var typeName = nonNullType switch
        {
            IArrayTypeSymbol at => at.ElementType.AsNotNullType().Name +
                                   (at.ElementType.IsNullable ? "Nullable" : "") + "Array",
            _ => nonNullType.Name,
        };
        return
            $"{interceptorTargetType}_{typeName}_{(isNullableType ? "Nullable" : "NotNull")}_Interception.g.cs";
    }
}
