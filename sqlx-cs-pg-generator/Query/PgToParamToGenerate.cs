using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sqlx.Postgres.Generator.Query;

internal readonly struct PgToParamToGenerate
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;

    public string ShortName => _typeSymbol.Name;

    public string ContainingNamespace { get; }

    public bool IsStruct => _typeSymbol.IsValueType;

    public Accessibility DeclaredAccessibility => _typeSymbol.DeclaredAccessibility;

    public ImmutableArray<IPropertySymbol> Properties { get; }

    public PgToParamToGenerate(
        INamedTypeSymbol namedTypeSymbol,
        TypeDeclarationSyntax typeDeclarationSyntax)
    {
        _typeSymbol = namedTypeSymbol;
        _typeDeclarationSyntax = typeDeclarationSyntax;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
        Properties = namedTypeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(property => !property.IsWriteOnly && property.IsNotSkip)
            .ToImmutableArray();
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

        var invalidProperties = Properties.Where(property => !property.Type.HasIPgDbType())
            .ToImmutableArray();
        if (!invalidProperties.IsEmpty)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    _typeDeclarationSyntax.GetLocation(),
                    $"Properties to bind have invalid DB types: [{string.Join(",", invalidProperties.Select(p => p.Name))}]"));
            return false;
        }

        return true;
    }
}
