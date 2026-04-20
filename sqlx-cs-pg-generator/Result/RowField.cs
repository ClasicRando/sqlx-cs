using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Result;

internal readonly struct RowField
{
    public string Name { get; }
    
    public string FieldName { get; }

    public ITypeSymbol FieldType { get; }
    
    public string IndexVariableName { get; }

    public bool Flatten { get; }

    public bool IsJson { get; }

    private RowField(string name, string fieldName, ITypeSymbol fieldType, bool flatten, bool isJson)
    {
        Name = name;
        FieldName = fieldName;
        FieldType = fieldType;
        IndexVariableName = $"index{fieldName.ToPascalCase()}";
        Flatten = flatten;
        IsJson = isJson;
    }

    private static RowField FromSymbol(
        ISymbol symbol,
        ITypeSymbol fieldType,
        Rename rename)
    {
        var attributes = symbol.GetAttributes();
        var fieldName = attributes.Where(attr => attr.AttributeClass!.Name == "PgNameAttribute")
            .Select(attr => (string)attr.ConstructorArguments[0].Value!)
            .FirstOrDefault() ?? rename.TransformName(symbol.Name);
        return new RowField(
            symbol.Name,
            fieldName,
            fieldType,
            attributes.Any(attr => attr.AttributeClass!.Name == "FlattenFieldAttribute"),
            attributes.Any(attr => attr.AttributeClass!.Name == "JsonFieldAttribute"));
    }

    public static RowField FromParameter(IParameterSymbol parameterSymbol, Rename rename)
    {
        return FromSymbol(parameterSymbol, parameterSymbol.Type, rename);
    }

    public static RowField FromProperty(IPropertySymbol propertySymbol, Rename rename)
    {
        return FromSymbol(propertySymbol, propertySymbol.Type, rename);
    }
}
