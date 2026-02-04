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

    public ImmutableArray<string> Properties { get; } =
        ImmutableArray<string>.Empty;

    public PgToParamToGenerate(
        INamedTypeSymbol namedTypeSymbol,
        TypeDeclarationSyntax typeDeclarationSyntax)
    {
        _typeSymbol = namedTypeSymbol;
        _typeDeclarationSyntax = typeDeclarationSyntax;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
        Properties = namedTypeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(property => !property.IsWriteOnly)
            .Where(property => !property.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name == "PgPropertySkipAttribute"))
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
                    Location.None,
                    ShortName));
            return false;
        }

        return true;
    }
}
