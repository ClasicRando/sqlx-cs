using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct WrapperEnum : IFullNameType
{
    private readonly INamedTypeSymbol _enumType;

    public WrapperEnum(INamedTypeSymbol namedTypeSymbol)
    {
        _enumType = namedTypeSymbol;
        var namedArguments = namedTypeSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.Name == "WrapperEnumAttribute")
            !.NamedArguments;
        Representation = (EnumRepresentation)namedArguments
            .FirstOrDefault(arg => arg.Key == "Representation")
            .Value
            .Value!;
        var renameAll = (Rename)(namedArguments
            .FirstOrDefault(arg => arg.Key == "RenameAll")
            .Value
            .Value ?? Rename.None);
        ValueNames = Representation is EnumRepresentation.Int
            ? ImmutableArray<KeyValuePair<string, string>>.Empty
            : namedTypeSymbol.GenerateFieldLookup(renameAll);
    }

    public string ShortName => _enumType.Name;

    public INamespaceSymbol ContainingNamespace => _enumType.ContainingNamespace;

    public EnumRepresentation Representation { get; }

    public INamedTypeSymbol EnumUnderlyingType => _enumType.EnumUnderlyingType!;

    public Accessibility DeclaredAccessibility => _enumType.DeclaredAccessibility;

    public string UniqueMethodName => string.IsNullOrEmpty(ContainingNamespace.FullName)
        ? "global_" + ShortName
        : ContainingNamespace.FullName.Replace('.', '_') + "_" + ShortName;

    public string UniqueMethodFullName => $"global::Sqlx.Postgres.Generator.Type.WrapperEnumTypes.{UniqueMethodName}";

    public ImmutableArray<KeyValuePair<string, string>> ValueNames { get; }
}
