using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator.Result;

public class MethodToGenerate
{
    private static readonly DiagnosticDescriptor MethodIsNotPartial =
        new(
            "SQLxPG001",
            "Annotated method is not partial",
            "'{0}' must be partial to allow for adding a method body",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);
    private static readonly DiagnosticDescriptor MethodIsNotExtensionTemplate =
        new(
            "SQLxPG002",
            "Annotated method is not an extension method",
            "'{0}' must be an extension method",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);
    private static readonly DiagnosticDescriptor MethodIsNotDecodeTemplate =
        new(
            "SQLxPG003",
            "Annotated method does not conform to the decode method template",
            "'{0}' must accept an IDataRow as this and a column indexer type. {1}",
            "sqlx-cs-pg Generation",
            DiagnosticSeverity.Error,
            true);
    private readonly IMethodSymbol _templateMethod;
    
    public MethodToGenerate(IMethodSymbol templateMethod)
    {
        _templateMethod = templateMethod;
        DecoderType = _templateMethod.GetAttributes()
            .FirstOrDefault()
            ?.NamedArguments
            .FirstOrDefault(arg => arg.Key == "Decoder")
            .Value
            .Value as ITypeSymbol;
    }

    public string Name => _templateMethod.Name;

    public ITypeSymbol ReturnType => SourceGenerationHelper.NotNullType(_templateMethod.ReturnType);

    public bool IsReturnNullable => _templateMethod.ReturnNullableAnnotation is NullableAnnotation.Annotated;

    public bool IsArrayReturn => _templateMethod.ReturnType.TypeKind is TypeKind.Array;

    private IParameterSymbol IndexerParameter => _templateMethod.Parameters[1];

    public ITypeSymbol IndexerParameterType => IndexerParameter.Type;

    public string IndexerParameterName => IndexerParameter.Name;
    
    public ITypeSymbol? DecoderType { get; }

    public bool Validate(SourceProductionContext context)
    {
        if (!_templateMethod.IsPartialDefinition)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(MethodIsNotPartial, Location.None, Name));
            return false;
        }
        
        if (!_templateMethod.IsExtensionMethod)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(MethodIsNotExtensionTemplate, Location.None, Name));
            return false;
        }

        if (_templateMethod.IsGenericMethod)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotDecodeTemplate,
                    Location.None, 
                    Name,
                    "Decode method must not be generic"));
            return false;
        }
        
        if (_templateMethod.Parameters.Length != 2)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotDecodeTemplate,
                    Location.None,
                    Name,
                    "Decode method must have exactly 2 parameters"));
            return false;
        }

        IParameterSymbol dataRowParameter = _templateMethod.Parameters[0];
        IParameterSymbol columnIndexerParameter = _templateMethod.Parameters[1];
        if (dataRowParameter.Type.Name != "IDataRow")
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotDecodeTemplate,
                    Location.None,
                    Name,
                    "First parameter must be IDataRow"));
            return false;
        }
        if (columnIndexerParameter.Type.ToDisplayString() != "string"
            && columnIndexerParameter.Type.ToDisplayString() != "int")
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    MethodIsNotDecodeTemplate,
                    Location.None,
                    Name,
                    "Second parameter must be indexer"));
            return false;
        }

        return true;
    }
}
