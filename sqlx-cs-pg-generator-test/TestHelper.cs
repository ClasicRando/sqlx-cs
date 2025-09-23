using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sqlx.Postgres.Generator.Query;
using Sqlx.Postgres.Generator.Result;

namespace Sqlx.Postgres.Generator.Tests;

public static class TestHelper
{
    public static Task VerifyPgDataRowDecoderGenerator(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        ];

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        var generator = new PgDataRowDecoderGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        return Verifier.Verify(driver)
            .UseDirectory("Snapshots");
    }
    
    public static Task VerifyPgExecutableBindGenerator(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        ];

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        var generator = new PgExecutableQueryBindGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        return Verifier.Verify(driver)
            .UseDirectory("Snapshots");
    }
}
