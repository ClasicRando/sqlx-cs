using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Sqlx.Postgres.Generator.Type;

namespace Sqlx.Postgres.Generator.Query;

public sealed class PgBindInterceptor : ISourceInterceptorPipeline<BindInvocation>
{
    public bool IsValidSyntax(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: "Bind",
            },
        };
    }

    public BindInvocation? CreateInterceptorContext(
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
                Name : "Bind",
                ContainingType.Name: "IPgBindable",
                TypeArguments: var typeArguments,
                Parameters: var parameters,
            }
            && typeArguments.Length == 1
            && typeArguments[0] is INamedTypeSymbol namedTypeSymbol
            && parameters.Length == 1)
        {
            if (context.SemanticModel.GetInterceptableLocation(invocation, cancellationToken) is
                { } location)
            {
                return new BindInvocation(location, namedTypeSymbol);
            }
        }

        return null;
    }

    public void ExecuteInterceptorGeneration(
        SourceProductionContext context,
        ImmutableArray<BindInvocation> item)
    {
        var sb = new StringBuilder();

        var encodeTypeGrouped = item.GroupBy(x => x.EncodeType, SymbolEqualityComparer.IncludeNullability);
        foreach (var encodeTypeGrouping in encodeTypeGrouped)
        {
            INamedTypeSymbol encodeType = encodeTypeGrouping.First().EncodeType;
            var isNullable = encodeType.IsNullable;
            INamedTypeSymbol nonNullEncodeType = encodeType.AsNotNullType();
            
            var iPgDbType = encodeType.GetIPgDbType();
            if (iPgDbType is not null)
            {
                GenerateIDbTypeInterceptor(
                    context,
                    sb,
                    encodeTypeGrouping,
                    nonNullEncodeType,
                    isNullable,
                    iPgDbType);
                continue;
            }
            
            if (nonNullEncodeType.IsWrapperEnum)
            {
                GenerateWrapperEnumInterceptor(
                    context,
                    sb,
                    encodeTypeGrouping,
                    nonNullEncodeType,
                    isNullable);
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    SourceGenerationHelper.UnknownDbType,
                    Location.None,
                    $"Calls to 'Bind' must resolve to a known DB type. SampleLocation {encodeTypeGrouping.First().Location.GetDisplayLocation()}"));
        }
    }

    private static void GenerateIDbTypeInterceptor(
        SourceProductionContext context,
        StringBuilder sb,
        IGrouping<ISymbol?, BindInvocation> encodeTypeGrouping,
        INamedTypeSymbol nonNullEncodeType,
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

        foreach (BindInvocation invocation in encodeTypeGrouping)
        {
            InterceptableLocation location = invocation.Location;
            var version = location.Version;
            var data = location.Data;
            var displayLocation = location.GetDisplayLocation();
            sb.AppendLine(
                $"""        [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}""");
        }

        sb.Append("        public static void Bind")
            .Append(nonNullEncodeType.Name)
            .Append(isNullable ? "Nullable" : "NotNull")
            .Append("(this global::Sqlx.Postgres.Query.IPgBindable pgBindable, ")
            .AppendFullName(nonNullEncodeType)
            .Append(isNullable ? "?" : string.Empty)
            .AppendLine(" value)");
        sb.AppendLine("        {");
        if (isNullable)
        {
            sb.AppendLine("            if (value is null)");
            sb.AppendLine("            {");
            sb.Append("                pgBindable.BindNull<")
                .AppendFullName(nonNullEncodeType)
                .AppendLine(">();");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.Append("                pgBindable.BindPg<")
                .AppendFullName(nonNullEncodeType)
                .Append(',')
                .Append(iPgDbType)
                .Append(">(value")
                .Append(nonNullEncodeType.IsValueType ? ".Value" : string.Empty)
                .AppendLine(");");
            sb.AppendLine("            }");
        }
        else
        {
            sb.Append("            pgBindable.BindPg<")
                .AppendFullName(nonNullEncodeType)
                .Append(',')
                .Append(iPgDbType)
                .AppendLine(">(value);");
        }
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        // Add the source to the compilation
        var contents = sb.ToString();
        var filename = $"IPgBindable_{nonNullEncodeType.Name}_{(isNullable ? "Nullable" : "NotNull")}_Interception.g.cs";
        context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
        sb.Clear();
    }

    private static void GenerateWrapperEnumInterceptor(
        SourceProductionContext context,
        StringBuilder sb,
        IGrouping<ISymbol?, BindInvocation> encodeTypeGrouping,
        INamedTypeSymbol nonNullEncodeType,
        bool isNullable)
    {
        var wrapperEnumToGenerate = new WrapperEnumToIntercept(nonNullEncodeType);
        
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

        foreach (BindInvocation invocation in encodeTypeGrouping)
        {
            InterceptableLocation location = invocation.Location;
            var version = location.Version;
            var data = location.Data;
            var displayLocation = location.GetDisplayLocation();
            sb.AppendLine(
                $"""        [global::System.Runtime.CompilerServices.InterceptsLocation({version}, "{data}")] // {displayLocation}""");
        }

        sb.Append("        public static void Bind")
            .Append(nonNullEncodeType.Name)
            .Append(isNullable ? "Nullable" : "NotNull")
            .Append("(this global::Sqlx.Postgres.Query.IPgBindable pgBindable, ")
            .AppendFullName(nonNullEncodeType)
            .Append(isNullable ? "?" : string.Empty)
            .AppendLine(" enumValue)");
        sb.AppendLine("        {");
        if (isNullable)
        {
            switch (wrapperEnumToGenerate.Representation)
            {
                case EnumRepresentation.Int:
                    sb.AppendLine("            if (enumValue.HasValue)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                pgBindable.BindPg<int, global::Sqlx.Postgres.Type.PgInt>((int)enumValue.Value);");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                pgBindable.BindNull<int>();");
                    sb.AppendLine("            }");
                    break;
                case EnumRepresentation.Text:
                    sb.AppendLine("            if (enumValue.HasValue)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                pgBindable.Bind(enumValue.Value.ToEncodeString());");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                    sb.AppendLine("                pgBindable.BindNull<string>();");
                    sb.AppendLine("            }");
                    break;
            }
        }
        else
        {
            switch (wrapperEnumToGenerate.Representation)
            {
                case EnumRepresentation.Int:
                    sb.AppendLine("            pgBindable.BindPg<int, global::Sqlx.Postgres.Type.PgInt>((int)enumValue);");
                    break;
                case EnumRepresentation.Text:
                    sb.AppendLine("            pgBindable.Bind(enumValue.ToEncodeString());");
                    break;
            }
        }
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        
        if (wrapperEnumToGenerate.Representation is EnumRepresentation.Text)
        {
            sb.AppendLine(
                """
                namespace Sqlx.Postgres.Interceptors
                {
                    static file class WrapperEnumTypes
                    {
                """);
            sb.Append("        extension(")
                .AppendFullName(nonNullEncodeType)
                .AppendLine(" enumValue)");
            sb.AppendLine("        {");
            sb.Append("            public global::System.ReadOnlySpan<char> ToEncodeString()");
            sb.AppendLine("            {");
            sb.AppendLine("                return enumValue switch");
            sb.AppendLine("                {");
            foreach (var kvp in wrapperEnumToGenerate.ValueNames)
            {
                sb.Append("                    ")
                    .AppendFullName(wrapperEnumToGenerate)
                    .Append('.')
                    .Append(kvp.Key)
                    .Append(" => \"")
                    .Append(kvp.Value)
                    .AppendLine("\",");
            }

            sb.AppendLine("                    _ => throw new global::System.InvalidOperationException($\"Attempted to encode an unknown enum variant, '{enumValue}'\"),");
            sb.AppendLine("                };");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Add the source to the compilation
        var contents = sb.ToString();
        var filename = $"IPgBindable_{nonNullEncodeType.Name}_{(isNullable ? "Nullable" : "NotNull")}_Interception.g.cs";
        context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
        sb.Clear();
    }
}
