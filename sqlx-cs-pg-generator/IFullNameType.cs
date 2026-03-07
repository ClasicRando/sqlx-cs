namespace Sqlx.Postgres.Generator;

internal interface IFullNameType
{
    string ShortName { get; }
    
    string ContainingNamespace { get; }
}
