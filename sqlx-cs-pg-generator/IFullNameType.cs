using Microsoft.CodeAnalysis;

namespace Sqlx.Postgres.Generator;

internal interface IFullNameType
{
    string ShortName { get; }
    
    INamespaceSymbol ContainingNamespace { get; }
}
