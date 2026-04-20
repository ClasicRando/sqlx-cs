using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct WrapperEnum : IFullNameType
{
    private readonly INamedTypeSymbol _enumType;

    public WrapperEnum(INamedTypeSymbol namedTypeSymbol)
    {
        _enumType = namedTypeSymbol;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
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
        if (Representation is EnumRepresentation.Int)
        {
            ValueNames = ImmutableArray<KeyValuePair<string, string>>.Empty;
        }
        else
        {
            ImmutableArray<KeyValuePair<string, string>>.Builder builder = ImmutableArray
                .CreateBuilder<KeyValuePair<string, string>>();

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

            ValueNames = builder.ToImmutable();
        }
    }

    public string ShortName => _enumType.Name;
    
    public string ContainingNamespace { get; }

    public EnumRepresentation Representation { get; }

    public INamedTypeSymbol EnumUnderlyingType => _enumType.EnumUnderlyingType!;

    public Accessibility DeclaredAccessibility => _enumType.DeclaredAccessibility;

    public string UniqueMethodName => string.IsNullOrEmpty(ContainingNamespace)
        ? "global_" + ShortName
        : ContainingNamespace.Replace('.', '_') + "_" + ShortName;

    public string UniqueMethodFullName => $"global::Sqlx.Postgres.Generator.Type.WrapperEnumTypes.{UniqueMethodName}";

    public ImmutableArray<KeyValuePair<string, string>> ValueNames { get; }
}
