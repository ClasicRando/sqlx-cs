using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator;

public interface ISourceInterceptorPipeline<T> where T : struct
{
    bool IsValidSyntax(SyntaxNode node, CancellationToken cancellationToken);

    T? CreateInterceptorContext(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken);

    void ExecuteInterceptorGeneration(SourceProductionContext context, ImmutableArray<T> item);
}
