using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct PgEnumToGenerate : IFullNameType
{
    private readonly INamedTypeSymbol _enumType;

    public PgEnumToGenerate(INamedTypeSymbol namedTypeSymbol)
    {
        _enumType = namedTypeSymbol;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
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

        List<KeyValuePair<string, string>> builder = [];
        foreach (IFieldSymbol? field in namedTypeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (field is null)
            {
                continue;
            }

            var name = field.Name;
            var overrideName = (string?)field
                .GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "PgNameAttribute")
                ?.ConstructorArguments
                .FirstOrDefault()
                .Value;

            var value = overrideName ?? renameAll.TransformName(name);
            builder.Add(new KeyValuePair<string, string>(name, value.Replace("\"", "\\\"")));
        }

        ValueNames = builder.ToImmutableArray();
    }

    public string ShortName => _enumType.Name;

    public string FullName => _enumType.FullName;
    
    public string ContainingNamespace { get; }

    public string TypeDefName => $"Pg{ShortName}";

    public string PgTypeName { get; }

    public Accessibility DeclaredAccessibility => _enumType.DeclaredAccessibility;

    public ImmutableArray<KeyValuePair<string, string>> ValueNames { get; }
}
