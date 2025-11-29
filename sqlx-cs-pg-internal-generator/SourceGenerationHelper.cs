using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator;

public static class SourceGenerationHelper
{
    public static readonly DiagnosticDescriptor MethodIsNotPartial =
        new(
            "SQLxPG001",
            "Annotated method is not partial",
            "'{0}' must be partial to allow for adding a method body",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);
    public static readonly DiagnosticDescriptor MethodIsNotExtension =
        new(
            "SQLxPG002",
            "Annotated method is not an extension method",
            "'{0}' must be an extension method",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);
    
    public static ITypeSymbol NotNullType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.NullableAnnotation is NullableAnnotation.NotAnnotated)
        {
            return typeSymbol;
        }

        return typeSymbol.Name.StartsWith("Nullable")
            ? ((INamedTypeSymbol)typeSymbol).TypeArguments[0]
            : typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
    }
}
