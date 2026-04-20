using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sqlx.Postgres.Generator.Query;

public readonly struct ExecuteScalarInvocation
{
    public ExecuteScalarInvocation(
        InterceptableLocation location,
        ITypeSymbol decodeType)
    {
        Location = location;
        DecodeType = decodeType;
    }

    public InterceptableLocation Location { get; }
    public ITypeSymbol DecodeType { get; }
}
