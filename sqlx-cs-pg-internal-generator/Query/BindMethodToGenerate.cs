using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Query;

public class BindMethodToGenerate
{
    private static readonly DiagnosticDescriptor MethodIsNotBindTemplate =
        new(
            "SQLxPG004",
            "Annotated method does not conform to the bind method template",
            "'{0}' must accept an IBindable as this and a value. {1}",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);
    private readonly IMethodSymbol _templateMethod;
    
    public BindMethodToGenerate(IMethodSymbol templateMethod)
    {
        _templateMethod = templateMethod;
        EncoderType = _templateMethod.GetAttributes()
            .FirstOrDefault()
            ?.NamedArguments
            .FirstOrDefault(arg => arg.Key == "Encoder")
            .Value
            .Value as ITypeSymbol;
    }

    public string Name => _templateMethod.Name;

    private IParameterSymbol ValueParameter => _templateMethod.Parameters[1];

    public string ValueParameterName => ValueParameter.Name;

    public ITypeSymbol ValueType => SourceGenerationHelper.NotNullType(ValueParameter.Type);

    public bool IsValueNullable => ValueParameter.Type.NullableAnnotation is NullableAnnotation.Annotated;

    public bool IsValueArray => ValueParameter.Type.TypeKind is TypeKind.Array;
    
    public ITypeSymbol? EncoderType { get; }

    public bool Validate(SourceProductionContext context)
    {
        if (!_templateMethod.IsPartialDefinition)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(SourceGenerationHelper.MethodIsNotPartial, Location.None, Name));
            return false;
        }
        
        if (!_templateMethod.IsExtensionMethod)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(SourceGenerationHelper.MethodIsNotExtension, Location.None, Name));
            return false;
        }

        if (_templateMethod.IsGenericMethod)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotBindTemplate,
                    Location.None, 
                    Name,
                    "Decode method must not be generic"));
            return false;
        }
        
        if (_templateMethod.Parameters.Length != 2)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotBindTemplate,
                    Location.None,
                    Name,
                    "Decode method must have exactly 2 parameters"));
            return false;
        }
        
        if (!_templateMethod.ReturnsVoid)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotBindTemplate,
                    Location.None,
                    Name,
                    "Decode method must have void return"));
            return false;
        }

        IParameterSymbol queryParameter = _templateMethod.Parameters[0];
        if (queryParameter.Type.Name != "IBindable")
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotBindTemplate,
                    Location.None,
                    Name,
                    "First parameter must be IBindable"));
            return false;
        }

        return true;
    }
}
