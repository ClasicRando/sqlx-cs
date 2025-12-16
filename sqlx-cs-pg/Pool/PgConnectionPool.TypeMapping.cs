using Sqlx.Core.Connection;
using Sqlx.Core.Query;
using Sqlx.Postgres.Exceptions;
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
        await using IConnection connection = CreateConnection();
        using IExecutableQuery typeOidQuery = connection.CreateQuery(pgEnumTypeByName);
        AddTypeNameAndSchemaToQuery<TType, TEnum>(typeOidQuery);
        
        try
        {
            var oid = await typeOidQuery.ExecuteScalarPg<PgOid>(cancellationToken);
            TType.DbType = new PgTypeInfo(oid.Inner, new EnumType());
        }
        catch (PgException e)
        {
            throw new PgException(
                "Failed to map enum. Make sure the type name is correct and include the schema name if necessary",
                e);
        }
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
        const string pgCompositeAttributeOidsByOid =
            """
            select a.attname, a.atttypid
            from pg_type t
            join pg_attribute a on t.typrelid = a.attrelid
            where
                t.oid = $1
                and t.typcategory = 'C'
                and a.attnum > 0
            """;
        await using IConnection connection = CreateConnection();
        using IExecutableQuery typeOidQuery = connection.CreateQuery(pgCompositeTypeByName);
        AddTypeNameAndSchemaToQuery<TComposite, TComposite>(typeOidQuery);

        PgOid oid;
        try
        {
            oid = await typeOidQuery.ExecuteScalarPg<PgOid>(cancellationToken);
        }
        catch (PgException e)
        {
            throw new PgException(
                "Failed to map composite. Make sure the type name is correct and include the schema name if necessary",
                e);
        }
        
        using IExecutableQuery attributeOidsQuery = connection.CreateQuery(pgCompositeAttributeOidsByOid);
        attributeOidsQuery.Bind(oid);
        
        var attributeOids = await attributeOidsQuery.Fetch<CompositeType.Attribute>(cancellationToken)
            .ToArrayAsync(cancellationToken);
        TComposite.DbType = new PgTypeInfo(
            oid.Inner,
            new CompositeType { Attributes = attributeOids});
    }

    private static void AddTypeNameAndSchemaToQuery<TType, TValue>(IBindable query)
        where TType : IPgUdt<TValue>
        where TValue : notnull
    {
        const string defaultSchemaName = "public";
        var typeName = TType.TypeName;
        var schemeQualifierIndex = typeName.IndexOf('.');
        if (schemeQualifierIndex > -1)
        {
            query.Bind(typeName[(schemeQualifierIndex + 1)..]);
            query.Bind(typeName[..schemeQualifierIndex]);
        }
        else
        {
            query.Bind(typeName);
            query.Bind(defaultSchemaName);
        }
    }
}
