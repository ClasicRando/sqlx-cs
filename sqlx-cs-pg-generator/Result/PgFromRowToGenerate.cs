using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sqlx.Postgres.Generator.Result;

internal readonly struct PgFromRowToGenerate : IFullNameType
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;

    public string ShortName => _typeSymbol.Name;

    public INamespaceSymbol ContainingNamespace => _typeSymbol.ContainingNamespace;

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
            ConstructorParameters = [
                ..primaryConstructor.Parameters
                    .Select(param => RowField.FromParameter(param, renameAll)),
            ];
        }

        if (primaryConstructor is null || (IsStruct && primaryConstructor.Parameters.IsEmpty))
        {
            InitProperties = [
                ..namedTypeSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(property => property.IsRequired || !property.IsReadOnly)
                    .Where(property => property.IsNotSkip)
                    .Select(property => RowField.FromProperty(property, renameAll)),
            ];
        }
    }

    public bool Validate(SourceProductionContext context)
    {
        if (!_typeDeclarationSyntax.IsPartial)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.DefinitionIsNotPartial,
                    _typeDeclarationSyntax.GetLocation(),
                    ShortName));
            return false;
        }

        var invalidParameterTypes = ConstructorParameters
            .Where(param => param is not { Flatten: true } and not { IsJson: true })
            .Where(param => !param.FieldType.HasIPgDbType())
            .ToImmutableArray();
        if (!invalidParameterTypes.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    _typeDeclarationSyntax.GetLocation(),
                    $"FromRow type declaration parameters that failed: {string.Join(",", invalidParameterTypes.Select(field => field.Name))}"));
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
                    _typeDeclarationSyntax.GetLocation(),
                    "parameter",
                    string.Join(",", invalidParameters.Select(field => field.Name))));
            return false;
        }

        var invalidPropertyTypes = InitProperties
            .Where(param => param is not { Flatten: true } and not { IsJson: true })
            .Where(property => !property.FieldType.HasIPgDbType())
            .ToImmutableArray();
        if (!invalidPropertyTypes.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    _typeDeclarationSyntax.GetLocation(),
                    $"FromRow type declaration properties that failed: {string.Join(",", invalidPropertyTypes.Select(field => field.Name))}"));
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
                    _typeDeclarationSyntax.GetLocation(),
                    "property",
                    string.Join(",", invalidProperties.Select(field => field.Name))));
            return false;
        }

        return true;
    }
}
