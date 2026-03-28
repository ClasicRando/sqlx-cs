using System.Runtime.CompilerServices;
using Sqlx.Core.Query;
using Sqlx.Postgres.Connection;
using Sqlx.Postgres.Exceptions;
using Sqlx.Postgres.Query;
using Sqlx.Postgres.Type;

namespace Sqlx.Postgres.Pool;

internal sealed partial class PgConnectionPool
{
    public async ValueTask MapEnumAsync<TEnum, TType>(CancellationToken cancellationToken = default)
        where TEnum : Enum
        where TType : IPgUdt<TEnum>
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
        IPgConnection connection = CreateConnection();
        await using ConfiguredAsyncDisposable _1 = connection.ConfigureAwait(false);
        IPgExecutableQuery typeOidQuery = connection.CreateQuery(pgEnumTypeByName);
        await using ConfiguredAsyncDisposable _2 = typeOidQuery.ConfigureAwait(false);
        AddTypeNameAndSchemaToQuery<TEnum, TType>(typeOidQuery);

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
        IPgConnection connection = CreateConnection();
        await using ConfiguredAsyncDisposable _1 = connection.ConfigureAwait(false);
        IPgExecutableQuery typeOidQuery = connection.CreateQuery(pgCompositeTypeByName);
        await using ConfiguredAsyncDisposable _2 = typeOidQuery.ConfigureAwait(false);
        AddTypeNameAndSchemaToQuery<TComposite, TComposite>(typeOidQuery);

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

        IPgExecutableQuery attributeOidsQuery =
            connection.CreateQuery(pgCompositeAttributeOidsByOid);
        await using ConfiguredAsyncDisposable _3 = attributeOidsQuery.ConfigureAwait(false);
        attributeOidsQuery.BindPg(oid);

        var attributeOids = await attributeOidsQuery
            .FetchAsync<CompositeField>(cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        TComposite.DbType = new PgTypeInfo(
            oid.Inner,
            new CompositeType { Fields = [..attributeOids] });
    }

    private static void AddTypeNameAndSchemaToQuery<TValue, TType>(IBindable query)
        where TValue : notnull
        where TType : IPgUdt<TValue>
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
