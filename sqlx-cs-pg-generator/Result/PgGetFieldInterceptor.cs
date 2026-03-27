using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Sqlx.Postgres.Generator.Result;

public sealed class PgGetFieldInterceptor : ISourceInterceptorPipeline<GetFieldInvocation>
{
    public bool IsValidSyntax(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: "GetField",
            },
        };
    }

    public GetFieldInvocation? CreateInterceptorContext(
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
                Name : "GetField",
                ContainingType.Name: "IPgDataRow",
                TypeArguments: var typeArguments,
                Parameters: var parameters,
            }
            && typeArguments.Length == 1
            && typeArguments[0] is INamedTypeSymbol namedTypeSymbol
            && parameters.Length == 1
            && parameters[0].Type is INamedTypeSymbol { Name: "Int32" or "String" } parameterType)
        {
            if (context.SemanticModel.GetInterceptableLocation(invocation, cancellationToken) is
                { } location)
            {
                return new GetFieldInvocation(location, namedTypeSymbol, parameterType.Name is "String");
            }
        }

        return null;
    }

    public void ExecuteInterceptorGeneration(
        SourceProductionContext context,
        ImmutableArray<GetFieldInvocation> item)
    {
        var sb = new StringBuilder();

        // group by the target enum.
        var decodeTypeGrouped = item.GroupBy(x => x.DecodeType, SymbolEqualityComparer.IncludeNullability);
        foreach (var decodeTypeGrouping in decodeTypeGrouped)
        {
            INamedTypeSymbol decodeType = decodeTypeGrouping.First().DecodeType;
            var iPgDbType = decodeType.GetIPgDbType();
            if (iPgDbType is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SourceGenerationHelper.UnknownDbType,
                        Location.None,
                        "Calls to 'GetField' must resolve to a known DB type"));
                continue;
            }

            var isNullable = decodeType.IsNullable;
            ITypeSymbol nonNullDecodeType = decodeType.AsNotNullType();
            
            sb.AppendLine("""
                        using Sqlx.Postgres.Result;
                        
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
                        
                        namespace Interceptor
                        {
                            static file class GetInterceptors
                            {
                        """);

            var parameterGrouped = decodeTypeGrouping.GroupBy(x => x.IsNameParameter);
            foreach (var parameterTypeGrouping in parameterGrouped)
            {
                var isNameParam = parameterTypeGrouping.Key;
                foreach (GetFieldInvocation invocation in parameterTypeGrouping)
                {
                    InterceptableLocation location = invocation.Location;
                    var version = location.Version;
                    var data = location.Data;
                    var displayLocation = location.GetDisplayLocation();
                    sb.AppendLine(
                        $"""        [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}""");
                }

                sb.Append("        public static ")
                    .AppendFullName(nonNullDecodeType);
                if (isNullable)
                {
                    sb.Append('?');
                }

                sb.Append(" Get")
                    .Append(nonNullDecodeType.Name)
                    .Append(isNullable ? "Nullable" : "NotNull")
                    .Append("(this IPgDataRow pgDataRow, ")
                    .Append(isNameParam ? "string name" : "int index")
                    .AppendLine(")");
                sb.AppendLine("        {");
                if (isNameParam)
                {
                    sb.AppendLine("            var index = pgDataRow.IndexOf(name);");
                }

                if (isNullable)
                {
                    sb.AppendLine("            if (pgDataRow.IsNull(index)) return null;");
                }
                sb.AppendLine($"            return pgDataRow.GetPgNotNull<{nonNullDecodeType.FullName}, {iPgDbType}>(index);");
                sb.AppendLine("        }");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");

            // Add the source to the compilation
            var contents = sb.ToString();
            var filename = $"IPgDataRow_{nonNullDecodeType.Name}_{(isNullable ? "Nullable" : "NotNull")}_Interception.g.cs";
            context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
            sb.Clear();
        }
    }
}
