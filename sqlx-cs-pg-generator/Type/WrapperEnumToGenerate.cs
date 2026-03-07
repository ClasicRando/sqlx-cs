using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Type;

internal readonly struct WrapperEnumToGenerate : IFullNameType
{
    private readonly INamedTypeSymbol _enumType;

    public WrapperEnumToGenerate(INamedTypeSymbol namedTypeSymbol)
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

    public ImmutableArray<KeyValuePair<string, string>> ValueNames { get; }

    public bool Validate(SourceProductionContext context)
    {
        if (Representation is EnumRepresentation.Int
            && _enumType.EnumUnderlyingType is not null
            && _enumType.EnumUnderlyingType?.ToString() != "int")
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.IntWrapperEnumNotIntBacked,
                    Location.None,
                    ShortName));
            return false;
        }

        return true;
    }
}
