using System.Text;
using Microsoft.CodeAnalysis;
using Sqlx.Postgres.Generator.Type;

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
    
    public static string GetFullNamespaceName(this INamespaceSymbol namespaceSymbol)
    {
        if (string.IsNullOrEmpty(namespaceSymbol.Name))
        {
            return string.Empty;
        }

        StringBuilder builder = new(namespaceSymbol.Name);
        INamespaceSymbol currentNamespace = namespaceSymbol.ContainingNamespace;
        while (!string.IsNullOrEmpty(currentNamespace.Name))
        {
            builder.Insert(0, '.');
            builder.Insert(0, currentNamespace.Name);
            currentNamespace = currentNamespace.ContainingNamespace;
        }

        return builder.ToString();
    }
    
    extension(StringBuilder builder)
    {
        public StringBuilder AppendFullPgEnumTypeName(PgEnumToGenerate pgEnumToGenerate)
        {
            if (!string.IsNullOrEmpty(pgEnumToGenerate.ContainingNamespace))
            {
                builder.Append(pgEnumToGenerate.ContainingNamespace);
                builder.Append('.');
            }

            builder.Append(pgEnumToGenerate.ShortName);
            return builder;
        }

        public StringBuilder AppendFullWrapperEnumTypeName(WrapperEnumToGenerate pgEnumToGenerate)
        {
            if (!string.IsNullOrEmpty(pgEnumToGenerate.ContainingNamespace))
            {
                builder.Append(pgEnumToGenerate.ContainingNamespace);
                builder.Append('.');
            }

            builder.Append(pgEnumToGenerate.ShortName);
            return builder;
        }
    }
}
