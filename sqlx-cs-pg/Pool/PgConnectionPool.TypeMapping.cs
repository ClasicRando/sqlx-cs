using Sqlx.Core.Query;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Pool;

internal sealed partial class PgConnectionPool
{
    public async Task MapEnumAsync<TType>(CancellationToken cancellationToken = default)
        where TType : Enum, IPgUdt<TType>
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
        using IPgConnection connection = CreateConnection();
        using IPgExecutableQuery typeOidQuery = connection.CreateQuery(pgEnumTypeByName);
        AddTypeNameAndSchemaToQuery<TType>(typeOidQuery);

        try
        {
            PgOid oid = await typeOidQuery.ExecuteScalar<PgOid>(cancellationToken)
                .ConfigureAwait(false);
            TType.DbType = new PgTypeInfo(oid.Inner, new EnumType());
        }
        catch (PgException e)
        {
            throw new PgException(
                "Failed to map enum. Make sure the type name is correct and include the schema name if necessary",
                e);
        }
    }

    public async Task MapCompositeAsync<TComposite>(CancellationToken cancellationToken = default)
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
        using IPgConnection connection = CreateConnection();
        using IPgExecutableQuery typeOidQuery = connection.CreateQuery(pgCompositeTypeByName);
        AddTypeNameAndSchemaToQuery<TComposite>(typeOidQuery);

        PgOid oid;
        try
        {
            oid = await typeOidQuery.ExecuteScalar<PgOid>(cancellationToken).ConfigureAwait(false);
        }
        catch (PgException e)
        {
            throw new PgException(
                "Failed to map composite. Make sure the type name is correct and include the schema name if necessary",
                e);
        }

        using IPgExecutableQuery attributeOidsQuery =
            connection.CreateQuery(pgCompositeAttributeOidsByOid);
        attributeOidsQuery.Bind(oid);

        var attributeOids = await attributeOidsQuery
            .FetchAsync<CompositeField>(cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        TComposite.DbType = new PgTypeInfo(
            oid.Inner,
            new CompositeType { Fields = [..attributeOids] });
    }

    private static void AddTypeNameAndSchemaToQuery<TType>(IBindable query)
        where TType : IPgUdt<TType>
    {
        const string defaultSchemaName = "public";
        var typeName = TType.TypeName;
        var schemeQualifierIndex = typeName.IndexOf('.', StringComparison.InvariantCulture);
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
