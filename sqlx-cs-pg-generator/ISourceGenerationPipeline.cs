using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator;

public interface ISourceGenerationPipeline<T> where T : struct
{
    string AttributeName { get; }

    bool IsValidSyntax(SyntaxNode node, CancellationToken cancellationToken);

    T? CreateGenerationContext(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken);

    void ExecuteGeneration(SourceProductionContext context, ImmutableArray<T> item);
}
