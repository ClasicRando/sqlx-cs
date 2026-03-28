using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sqlx.Postgres.Generator.Query;

public readonly record struct BindInvocation
{
    public BindInvocation(
        InterceptableLocation location,
        INamedTypeSymbol encodeType)
    {
        Location = location;
        EncodeType = encodeType;
    }

    public InterceptableLocation Location { get; }
    public INamedTypeSymbol EncodeType { get; }
}
