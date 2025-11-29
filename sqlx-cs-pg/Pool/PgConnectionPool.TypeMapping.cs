using Sqlx.Core.Query;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Pool;

public sealed partial class PgConnectionPool
{
    public async Task MapEnum<TEnum, TType>(CancellationToken cancellationToken = default)
        where TType : IPgUdt<TEnum>
        where TEnum : Enum
    {
        const string pgEnumTypeByName =
            """
            SELECT t.oid
            FROM pg_type t
            JOIN pg_namespace n ON t.typnamespace = n.oid
            WHERE
                t.typname = $1
                AND n.nspname = $2
                AND t.typcategory = 'E'
            """;
        using IExecutableQuery typeOidQuery = CreateQuery(pgEnumTypeByName);
        AddTypeNameAndSchemaToQuery<TType, TEnum>(typeOidQuery);
        
        var oid = await typeOidQuery.ExecuteScalarPg<PgOid>(cancellationToken);
        
        TType.DbType = new PgTypeInfo(oid.Inner, new EnumType());
    }

    public async Task MapComposite<TComposite>(CancellationToken cancellationToken = default)
        where TComposite : IPgUdt<TComposite>
    {
        const string pgCompositeTypeByName =
            """
            SELECT t.oid
            FROM pg_type t
            JOIN pg_namespace n ON t.typnamespace = n.oid
            WHERE
                t.typname = $1
                AND n.nspname = $2
                AND t.typcategory = 'C'
            """;
        using IExecutableQuery typeOidQuery = CreateQuery(pgCompositeTypeByName);
        AddTypeNameAndSchemaToQuery<TComposite, TComposite>(typeOidQuery);
        
        var oid = await typeOidQuery.ExecuteScalarPg<PgOid>(cancellationToken);
        
        TComposite.DbType = new PgTypeInfo(oid.Inner, new CompositeType());
    }

    private static void AddTypeNameAndSchemaToQuery<TType, TValue>(IQuery query)
        where TType : IPgUdt<TValue>
        where TValue : notnull
    {
        const string defaultSchemaName = "public";
        var typeName = TType.TypeName;
        var schemeQualifierIndex = typeName.IndexOf('.');
        if (schemeQualifierIndex > -1)
        {
            query.Bind(typeName.AsSpan()[(schemeQualifierIndex + 1)..]);
            query.Bind(typeName.AsSpan()[..schemeQualifierIndex]);
        }
        else
        {
            query.Bind(typeName);
            query.Bind(defaultSchemaName);
        }
    }
}
