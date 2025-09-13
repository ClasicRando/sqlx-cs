using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator;

public static class SourceGenerationHelper
{
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
