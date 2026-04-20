using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Sqlx.Postgres.Generator.Type;

namespace Sqlx.Postgres.Generator.Query;

public class PgExecuteScalarInterceptor : ISourceInterceptorPipeline<ExecuteScalarInvocation>
{
    public bool IsValidSyntax(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: "ExecuteScalar",
            },
        };
    }

    public ExecuteScalarInvocation? CreateInterceptorContext(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        if (context.Node is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax,
            } invocation
            && context.SemanticModel.GetOperation(context.Node, cancellationToken) is
                IInvocationOperation targetOperation
            && targetOperation.TargetMethod is
            {
                Name : "ExecuteScalar",
                ContainingType.Name: "IPgExecutableQuery",
                TypeArguments: var typeArguments,
                Parameters: var parameters,
            }
            && typeArguments.Length == 1
            && parameters.Length == 1
            && parameters[0].Type is INamedTypeSymbol { Name: "CancellationToken" })
        {
            if (context.SemanticModel.GetInterceptableLocation(invocation, cancellationToken) is
                { } location)
            {
                return new ExecuteScalarInvocation(location, typeArguments[0]);
            }
        }

        return null;
    }

    public void ExecuteInterceptorGeneration(SourceProductionContext context, ImmutableArray<ExecuteScalarInvocation> item)
    {
        var sb = new StringBuilder();

        var decodeTypeGrouped = item.GroupBy(x => x.DecodeType, SymbolEqualityComparer.IncludeNullability);
        foreach (var decodeTypeGrouping in decodeTypeGrouped)
        {
            ITypeSymbol decodeType = decodeTypeGrouping.First().DecodeType;
            
            var iPgDbType = decodeType.GetIPgDbType();
            if (iPgDbType is not null)
            {
                GenerateIDbTypeInterceptor(
                    context,
                    sb,
                    decodeTypeGrouping,
                    decodeType,
                    iPgDbType);
                continue;
            }
            
            if (decodeType is INamedTypeSymbol { IsWrapperEnum: true } nt)
            {
                GenerateWrapperEnumInterceptor(
                    context,
                    sb,
                    decodeTypeGrouping,
                    nt);
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    Location.None,
                    $"Calls to 'ExecuteScalar' must resolve to a known DB type. SampleLocation {decodeTypeGrouping.First().Location.GetDisplayLocation()}"));
        }
    }
    
    private static void GenerateIDbTypeInterceptor(
        SourceProductionContext context,
        StringBuilder sb,
        IGrouping<ISymbol?, ExecuteScalarInvocation> decodeTypeGrouping,
        ITypeSymbol nonNullDecodeType,
        string iPgDbType)
    {
        sb.AppendLine("""
            #nullable enable
            namespace System.Runtime.CompilerServices
            {
                [global::System.Diagnostics.Conditional("DEBUG")]
                [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                sealed file class InterceptsLocationAttribute : global::System.Attribute
                {
                    public InterceptsLocationAttribute(int version, string data)
                    {
                        _ = version;
                        _ = data;
                    }
                }
            }
            
            namespace Sqlx.Postgres.Interceptors
            {
                static file class GetInterceptors
                {
            """);

        foreach (ExecuteScalarInvocation invocation in decodeTypeGrouping)
        {
            InterceptableLocation location = invocation.Location;
            var version = location.Version;
            var data = location.Data;
            var displayLocation = location.GetDisplayLocation();
            sb.AppendLine(
                $"""        [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}""");
        }

        sb.Append("        public static Task<")
            .AppendFullName(nonNullDecodeType)
            .Append("> ExecuteScalar")
            .Append(nonNullDecodeType.Name)
            .AppendLine("(this global::Sqlx.Postgres.Query.IPgExecutableQuery pgExecutableQuery, CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        sb.Append("            return global::Sqlx.Postgres.Query.ExecutableQuery.ExecuteScalarPg<")
            .AppendFullName(nonNullDecodeType)
            .Append(',')
            .Append(iPgDbType)
            .AppendLine(">(pgExecutableQuery, cancellationToken);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // Add the source to the compilation
        var contents = sb.ToString();
        var filename = SourceGenerationHelper.GetSourceInterceptorFileName(
            "IPgExecutableQuery",
            nonNullDecodeType,
            false);
        context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
        sb.Clear();
    }

    private static void GenerateWrapperEnumInterceptor(
        SourceProductionContext context,
        StringBuilder sb,
        IGrouping<ISymbol?, ExecuteScalarInvocation> decodeTypeGrouping,
        INamedTypeSymbol nonNullDecodeType)
    {
        var wrapperEnumToIntercept = new WrapperEnum(nonNullDecodeType);
        
        sb.AppendLine("""
            #nullable enable
            namespace System.Runtime.CompilerServices
            {
                [global::System.Diagnostics.Conditional("DEBUG")]
                [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                sealed file class InterceptsLocationAttribute : global::System.Attribute
                {
                    public InterceptsLocationAttribute(int version, string data)
                    {
                        _ = version;
                        _ = data;
                    }
                }
            }
            
            namespace Sqlx.Postgres.Interceptors
            {
                static file class GetInterceptors
                {
            """);

        foreach (ExecuteScalarInvocation invocation in decodeTypeGrouping)
        {
            InterceptableLocation location = invocation.Location;
            var version = location.Version;
            var data = location.Data;
            var displayLocation = location.GetDisplayLocation();
            sb.AppendLine(
                $"""        [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}""");
        }
        
        sb.Append("        public static async Task<")
            .AppendFullName(nonNullDecodeType)
            .Append("> ExecuteScalar")
            .Append(nonNullDecodeType.Name)
            .Append("(this global::Sqlx.Postgres.Query.IPgExecutableQuery pgExecutableQuery, CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        switch (wrapperEnumToIntercept.Representation)
        {
            case EnumRepresentation.Int:
                sb.Append("            return (")
                    .AppendFullName(wrapperEnumToIntercept)
                    .AppendLine(")await global::Sqlx.Postgres.Query.ExecutableQuery.ExecuteScalarPg<int, global::Sqlx.Postgres.Type.PgInt>(pgExecutableQuery, cancellationToken);");
                break;
            case EnumRepresentation.Text:
                sb.Append("            return ")
                    .Append(wrapperEnumToIntercept.UniqueMethodFullName)
                    .AppendLine("_FromChars(await global::Sqlx.Postgres.Query.ExecutableQuery.ExecuteScalarPg<string, global::Sqlx.Postgres.Type.PgString>(pgExecutableQuery, cancellationToken));");
                break;
        }
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // Add the source to the compilation
        var contents = sb.ToString();
        var filename = SourceGenerationHelper.GetSourceInterceptorFileName(
            "IPgExecutableQuery",
            nonNullDecodeType,
            false);
        context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
        sb.Clear();
    }
}
