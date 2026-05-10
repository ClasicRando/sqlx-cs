using System.Net;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sqlx.Core.Types;
using Sqlx.Postgres.Connection;

namespace Sqlx.Postgres.Generator.Tests;

public static class TestHelper
{
    public static Task VerifyPostgresGenerator(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(JsonElement).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IPNetwork).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(JsonValue<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(PgConnection).Assembly.Location),
        ];

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        var generator = new PostgresGenerator();
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
            .RunGenerators(compilation);

        return Verify(driver)
            .UseDirectory("Snapshots");
    }
}
