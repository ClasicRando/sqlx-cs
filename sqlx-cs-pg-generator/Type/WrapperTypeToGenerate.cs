using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sqlx.Postgres.Generator.Type;

public readonly record struct WrapperTypeToGenerate : IFullNameType
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly TypeDeclarationSyntax _typeDeclarationSyntax;

    public string ShortName => _typeSymbol.Name;

    public string ContainingNamespace { get; }

    public Accessibility DeclaredAccessibility => _typeSymbol.DeclaredAccessibility;

    public bool IsStruct => _typeSymbol.IsValueType;
    
    private ImmutableArray<IPropertySymbol> Properties { get; }

    public IPropertySymbol InnerProperty => Properties[0];

    public bool HasNonDefaultConstructor => _typeSymbol.InstanceConstructors
        .Any(ctor => !ctor.Parameters.IsEmpty);

    public WrapperTypeToGenerate(
        INamedTypeSymbol namedTypeSymbol,
        TypeDeclarationSyntax typeDeclarationSyntax)
    {
        _typeSymbol = namedTypeSymbol;
        _typeDeclarationSyntax = typeDeclarationSyntax;
        ContainingNamespace = namedTypeSymbol.ContainingNamespace.GetFullNamespaceName();
        Properties = _typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(property => property.IsNotSkip)
            .ToImmutableArray();
    }
    
    public bool Validate(SourceProductionContext context)
    {
        if (!_typeDeclarationSyntax.IsPartial)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.DefinitionIsNotPartial,
                    _typeDeclarationSyntax.GetLocation(),
                    ShortName));
            return false;
        }
        
        if (!_typeSymbol.IsValueType)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.DefinitionShouldBeValueType,
                    _typeDeclarationSyntax.GetLocation(),
                    ShortName));
        }

        if (Properties.Length != 1)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.InvalidTypeDefinition,
                    _typeDeclarationSyntax.GetLocation(),
                    ShortName,
                    "WrapperType",
                    "The number of non-skipped properties must be exactly 1."));
            return false;
        }

        IPropertySymbol property = InnerProperty;
        if (!property.Type.HasIPgDbType())
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.InvalidTypeDefinition,
                    _typeDeclarationSyntax.GetLocation(),
                    ShortName,
                    "WrapperType",
                    "The inner property type is not a valid type that has a corresponding IPgDbType"));
            return false;
        }
        
        if (property.Type.NullableAnnotation is NullableAnnotation.Annotated)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.InvalidTypeDefinition,
                    _typeDeclarationSyntax.GetLocation(),
                    ShortName,
                    "WrapperType",
                    "The inner property type must not be nullable. Make usages of the type nullable."));
            return false;
        }

        return true;
    }
}
