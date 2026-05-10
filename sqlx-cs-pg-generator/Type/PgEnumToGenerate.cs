using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct PgEnumToGenerate : IFullNameType
{
    private readonly INamedTypeSymbol _enumType;

    public PgEnumToGenerate(INamedTypeSymbol namedTypeSymbol)
    {
        _enumType = namedTypeSymbol;
        var namedArguments = namedTypeSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.Name == "PgEnumAttribute")
            !.NamedArguments;
        PgTypeName = (string)namedArguments
            .FirstOrDefault(arg => arg.Key == "Name")
            .Value
            .Value!;
        var renameAll = (Rename)(namedArguments
            .FirstOrDefault(arg => arg.Key == "RenameAll")
            .Value
            .Value ?? Rename.None);

        ValueNames = namedTypeSymbol.GenerateFieldLookup(renameAll);
    }

    public string ShortName => _enumType.Name;

    public string FullName => _enumType.FullName;

    public INamespaceSymbol ContainingNamespace => _enumType.ContainingNamespace;

    public string TypeDefName => $"Pg{ShortName}";

    public string PgTypeName { get; }

    public Accessibility DeclaredAccessibility => _enumType.DeclaredAccessibility;

    public ImmutableArray<KeyValuePair<string, string>> ValueNames { get; }
}
