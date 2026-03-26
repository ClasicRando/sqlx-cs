using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sqlx.Postgres.Generator.Result;

internal readonly struct PgFromRowToGenerate
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;

    public string ShortName => _typeSymbol.Name;

    public string ContainingNamespace { get; }

    public bool IsStruct => _typeSymbol.IsValueType;

    public Accessibility DeclaredAccessibility => _typeSymbol.DeclaredAccessibility;

    public ImmutableArray<RowField> ConstructorParameters { get; } =
        ImmutableArray<RowField>.Empty;

    public ImmutableArray<RowField> InitProperties { get; } =
        ImmutableArray<RowField>.Empty;

    public PgFromRowToGenerate(
        INamedTypeSymbol namedTypeSymbol,
        TypeDeclarationSyntax typeDeclarationSyntax)
    {
        _typeSymbol = namedTypeSymbol;
        _typeDeclarationSyntax = typeDeclarationSyntax;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
        var renameAll = (Rename)(namedTypeSymbol.GetAttributes()
            .Select(attr => attr.NamedArguments
                .Where(arg => arg.Key == "RenameAll")
                .Select(arg => arg.Value.Value)
                .FirstOrDefault())
            .FirstOrDefault(v => v is not null) ?? Rename.None);
        IMethodSymbol? primaryConstructor = namedTypeSymbol.InstanceConstructors
            .OrderByDescending(method => method.Parameters.Length)
            .FirstOrDefault();
        if (primaryConstructor is not null)
        {
            ConstructorParameters = primaryConstructor.Parameters
                .Select(param => RowField.FromParameter(param, renameAll))
                .ToImmutableArray();
        }

        if (primaryConstructor is null || (IsStruct && primaryConstructor.Parameters.IsEmpty))
        {
            InitProperties = namedTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(property => property.IsRequired || !property.IsReadOnly)
                .Where(property => !property.GetAttributes().Any(attr =>
                    attr.AttributeClass?.Name == "PgPropertySkipAttribute"))
                .Select(property => RowField.FromProperty(property, renameAll))
                .ToImmutableArray();
        }
    }

    public bool Validate(SourceProductionContext context)
    {
        if (!_typeDeclarationSyntax.IsPartial)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.DefinitionIsNotPartial,
                    Location.None,
                    ShortName));
            return false;
        }

        var invalidParameterTypes = ConstructorParameters
            .Where(param => param is not { Flatten: true } and not { IsJson: true })
            .Where(param => !param.FieldType.IsValidDbType())
            .ToImmutableArray();
        if (!invalidParameterTypes.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    Location.None,
                    "parameter",
                    string.Join(",", invalidParameterTypes.Select(field => field.Name))));
            return false;
        }

        var invalidParameters = ConstructorParameters
            .Where(param => param is { Flatten: true, IsJson: true })
            .ToImmutableArray();
        if (!invalidParameters.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.ExcessiveFieldAttributes,
                    Location.None,
                    "parameter",
                    string.Join(",", invalidParameters.Select(field => field.Name))));
            return false;
        }

        var invalidPropertyTypes = InitProperties
            .Where(param => param is not { Flatten: true } and not { IsJson: true })
            .Where(property => !property.FieldType.IsValidDbType())
            .ToImmutableArray();
        if (!invalidPropertyTypes.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    Location.None,
                    "property",
                    string.Join(",", invalidPropertyTypes.Select(field => field.Name))));
            return false;
        }

        var invalidProperties = InitProperties
            .Where(property => property is { Flatten: true, IsJson: true })
            .ToImmutableArray();
        if (!invalidProperties.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.ExcessiveFieldAttributes,
                    Location.None,
                    "property",
                    string.Join(",", invalidProperties.Select(field => field.Name))));
            return false;
        }

        return true;
    }
}
