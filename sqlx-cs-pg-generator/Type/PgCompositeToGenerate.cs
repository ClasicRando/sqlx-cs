using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sqlx.Postgres.Generator.Query;
using Sqlx.Postgres.Generator.Result;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct PgCompositeToGenerate : IFullNameType
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;

    public string ShortName => _typeSymbol.Name;

    public string ContainingNamespace { get; }

    public bool IsStruct => _typeSymbol.IsValueType;

    public Accessibility DeclaredAccessibility => _typeSymbol.DeclaredAccessibility;
    
    public bool IsIFromRow { get; }
    
    public bool IsIBindMany { get; }

    public string PgTypeName { get; }

    public PgCompositeToGenerate(
        INamedTypeSymbol namedTypeSymbol,
        TypeDeclarationSyntax typeDeclarationSyntax)
    {
        _typeSymbol = namedTypeSymbol;
        _typeDeclarationSyntax = typeDeclarationSyntax;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
        IsIFromRow = namedTypeSymbol.AllInterfaces.Any(i => i.Name.StartsWith("IFromRow"));
        IsIBindMany = namedTypeSymbol.AllInterfaces.Any(i => i.Name.StartsWith("IBindMany"));
        var namedArguments = namedTypeSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.Name == "PgCompositeAttribute")
            !.NamedArguments;
        PgTypeName = (string)namedArguments
            .FirstOrDefault(arg => arg.Key == "Name")
            .Value
            .Value!;
    }

    public PgFromRowToGenerate GetPgFromRowToGenerate()
    {
        return new PgFromRowToGenerate(_typeSymbol, _typeDeclarationSyntax);
    }

    public PgToParamToGenerate GetPgToParamToGenerate()
    {
        return new PgToParamToGenerate(_typeSymbol, _typeDeclarationSyntax);
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
