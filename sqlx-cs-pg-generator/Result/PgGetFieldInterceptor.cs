using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Sqlx.Postgres.Generator.Type;

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

        var decodeTypeGrouped = item.GroupBy(x => x.DecodeType, SymbolEqualityComparer.IncludeNullability);
        foreach (var decodeTypeGrouping in decodeTypeGrouped)
        {
            INamedTypeSymbol decodeType = decodeTypeGrouping.First().DecodeType;
            var isNullable = decodeType.IsNullable;
            INamedTypeSymbol nonNullDecodeType = decodeType.AsNotNullType();

            var iPgDbType = decodeType.GetIPgDbType();
            if (iPgDbType is not null)
            {
                GenerateIDbTypeInterceptor(
                    context,
                    sb,
                    decodeTypeGrouping,
                    nonNullDecodeType,
                    isNullable,
                    iPgDbType);
                continue;
            }

            if (nonNullDecodeType.IsWrapperEnum)
            {
                GenerateWrapperEnumInterceptor(
                    context,
                    sb,
                    decodeTypeGrouping,
                    nonNullDecodeType,
                    isNullable);
                continue;
            }
            
            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    Location.None,
                    "Calls to 'GetField' must resolve to a known DB type"));
        }
    }

    private static void GenerateIDbTypeInterceptor(
        SourceProductionContext context,
        StringBuilder sb,
        IGrouping<ISymbol?, GetFieldInvocation> decodeTypeGrouping,
        INamedTypeSymbol nonNullDecodeType,
        bool isNullable,
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
                .Append("(this global::Sqlx.Postgres.Result.IPgDataRow pgDataRow, ")
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
    
    private static void GenerateWrapperEnumInterceptor(
        SourceProductionContext context,
        StringBuilder sb,
        IGrouping<ISymbol?, GetFieldInvocation> decodeTypeGrouping,
        INamedTypeSymbol nonNullDecodeType,
        bool isNullable)
    {
        var wrapperEnumToIntercept = new WrapperEnumToIntercept(nonNullDecodeType);
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
                .Append("(this global::Sqlx.Postgres.Result.IPgDataRow pgDataRow, ")
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
            switch (wrapperEnumToIntercept.Representation)
            {
                case EnumRepresentation.Int:
                    sb.Append("            return (")
                        .AppendFullName(wrapperEnumToIntercept)
                        .AppendLine(")pgDataRow.GetPgNotNull<int, global::Sqlx.Postgres.Type.PgInt>(index);");
                    break;
                case EnumRepresentation.Text:
                    sb.Append("            return ")
                        .AppendFullName(wrapperEnumToIntercept)
                        .AppendLine(".FromChars(pgDataRow.GetPgNotNull<string, global::Sqlx.Postgres.Type.PgString>(index));");
                    break;
            }
            sb.AppendLine("        }");
        }
        
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        if (wrapperEnumToIntercept.Representation is EnumRepresentation.Text)
        {
            sb.AppendLine(
                """
                namespace Sqlx.Postgres.Interceptors
                {
                    static file class WrapperEnumTypes
                    {
                """);
            sb.Append("        extension(")
                .AppendFullName(nonNullDecodeType)
                .AppendLine(" enumValue)");
            sb.AppendLine("        {");
            sb.Append("            public static ")
                .AppendFullName(wrapperEnumToIntercept)
                .AppendLine(" FromChars(in global::System.ReadOnlySpan<char> chars)");
            sb.AppendLine("            {");
            sb.AppendLine("                return chars switch");
            sb.AppendLine("                {");
            foreach (var kvp in wrapperEnumToIntercept.ValueNames)
            {
                sb.Append("                    \"")
                    .Append(kvp.Value)
                    .Append("\" => ")
                    .AppendFullName(wrapperEnumToIntercept)
                    .Append('.')
                    .Append(kvp.Key)
                    .AppendLine(",");
            }

            sb.AppendLine("                    _ => throw new global::System.InvalidOperationException($\"Attempted to decode an unknown enum variant with name, '{chars}'\"),");
            sb.AppendLine("                };");
            sb.AppendLine("            }");

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Add the source to the compilation
        var contents = sb.ToString();
        var filename = $"IPgDataRow_{nonNullDecodeType.Name}_{(isNullable ? "Nullable" : "NotNull")}_Interception.g.cs";
        context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
        sb.Clear();
    }
}
