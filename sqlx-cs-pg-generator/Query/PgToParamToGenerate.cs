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

    public ImmutableArray<string> Properties { get; }

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
            .Select(property => property.Name)
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

        return true;
    }
}
