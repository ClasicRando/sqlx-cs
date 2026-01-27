using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sqlx.Postgres.Generator.Type;

namespace Sqlx.Postgres.Generator.Tests;

public static class TestHelper
{
    public static Task VerifyPgEnumImplementationGenerator(string source)
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

        var generator = new PgEnumImplementationGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        return Verify(driver)
            .UseDirectory("Snapshots");
    }
}
