using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sqlx.Postgres.Generator.Result;

public readonly record struct GetFieldInvocation
{
    public GetFieldInvocation(
        InterceptableLocation location,
        INamedTypeSymbol decodeType,
        bool isNameParameter)
    {
        Location = location;
        DecodeType = decodeType;
        IsNameParameter = isNameParameter;
    }

    public InterceptableLocation Location { get; }
    public INamedTypeSymbol DecodeType { get; }
    public bool IsNameParameter { get; }
}
