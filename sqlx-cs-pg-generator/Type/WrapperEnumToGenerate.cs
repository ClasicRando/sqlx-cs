using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct WrapperEnumToGenerate
{
    private readonly EnumDeclarationSyntax _enumDeclarationSyntax;
    
    public WrapperEnum Inner { get; }
    
    public WrapperEnumToGenerate(
        INamedTypeSymbol enumSymbol,
        EnumDeclarationSyntax enumDeclarationSyntax)
    {
        Inner = new WrapperEnum(enumSymbol);
        _enumDeclarationSyntax = enumDeclarationSyntax;
    }
    public bool Validate(SourceProductionContext context)
    {
        WrapperEnum inner = Inner;
        if (inner.Representation is EnumRepresentation.Int
            && inner.EnumUnderlyingType.ToString() != "int")
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.IntWrapperEnumNotIntBacked,
                    _enumDeclarationSyntax.GetLocation(),
                    inner.ShortName));
            return false;
        }

        return true;
    }
}
