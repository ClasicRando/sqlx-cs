# Postgres
## Connection Options
These options are provided to a connection pool to use for every new connection needed.

| Option                              | Description                                                                                                                                                                                                                                                                                                            | Default                     |
|-------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------|
| Host                                | Host name/address of the database server                                                                                                                                                                                                                                                                               | required                    |
| Port                                | Host port to connect to                                                                                                                                                                                                                                                                                                | 5432                        |
| Username                            | User to connect with as                                                                                                                                                                                                                                                                                                | required                    |
| ApplicationName                     | `application_name` property set for each connection                                                                                                                                                                                                                                                                    | sqlx-cs-driver              |
| Password                            | Password provided for the user connecting. This will become optional in the future if/when password-less connection are enabled for this driver (e.g. GSS-API, SSPI, OAuth tokens)                                                                                                                                     | required                    |
| Database                            | Optional database name to initialize connections with. By default, Postgres will attempt to connect to a database with the same name as the username.                                                                                                                                                                  | required                    |
| ConnectTimeout                      | Timeout to use when creating a new connection to the database. Must be postive and non-zero. Use `Timeout.InfiniteTimeSpan` to disable timeout                                                                                                                                                                         | 15s                         |
| QueryTimeout                        | Timeout to use when executing a query(s) against the database. Must be postive and non-zero. Use `Timeout.InfiniteTimeSpan` to disable timeout. Currently this sets the `statement_timeout` connection property upon intialization so it's not currently configurable per query.                                       | Infinite                    |
| StatementCacheCapacity              | Size of the prepared statement cache. Setting a larger size will allow for more. Setting a larger size will allow for more statements to be executed without parsing again, but it will accumulate more statements on the server side which could impact performance of the server.                                    | 100                         |
| UseExtendedProtocolForSimpleQueries | True if the driver should execute some simple statements as prepared queries. This does not impact simple statements that contain multiple queries or characters that look like parameter placeholders. This generally improves performance by using binary encoding for results but also might slow down performance. | true                        |
| SslMode                             | !CURRENTLY DOES NOTHING! SSL behaviour for connecting to databases that support SSL connection.                                                                                                                                                                                                                        | `SslMode.Prefer`            |
| ExtraFloatPoints                    | This should rarely if ever be changed. Consulte [docs](https://www.postgresql.org/docs/current/runtime-config-client.html#GUC-EXTRA-FLOAT-DIGITS) for more details.                                                                                                                                                    | 1                           |
| CurrentSchema                       | Default schema to use after connecting. Sets the `search_path` connection property of the connections.                                                                                                                                                                                                                 | n/a                         |
| SslMode                             | !CURRENTLY DOES NOTHING! SASL-PLUS channel bidning behaviour for connecting to databases using SASL over an SSL connection.                                                                                                                                                                                            | `ChannelBinding.Prefer`     |
| LoggerFactory                       | Logger creation factory of type `Microsoft.Extensions.Logging.ILoggerFactory` used by all objects that create loggers.                                                                                                                                                                                                 | Factory with console logger |

## Type Mapping
| CLR Type                       | Postgres Type                                |
|--------------------------------|----------------------------------------------|
| bool                           | BOOLEAN                                      |
| sbyte                          | "CHAR"                                       |
| short                          | SMALLINT                                     |
| int                            | INTEGER                                      |
| long                           | BIGINT                                       |
| float                          | REAL                                         |
| double                         | DOUBLE PRECISION                             |
| TimeOnly                       | TIME                                         |
| DateOnly                       | DATE                                         |
| DateTime                       | TIMESTAMP, TIMESTAMP WITH TIME ZONE          |
| DateTimeOffset                 | TIMESTAMP WITH TIME ZONE, TIMESTAMP          |
| DateOnly                       | DATE                                         |
| decimal                        | NUMERIC(x, y)                                |
| byte[]                         | BYTEA                                        |
| string                         | TEXT, VARCHAR(x), CHAR(x), NAME, BPCHAR, XML |
| Guid                           | UUID                                         |
| IPNetwork                      | CIDR, INET                                   |
| BitArray                       | VARBIT(x), BIT(x)                            |
| PgRange&lt;long&gt;*           | INT8RANGE                                    |
| PgRange&lt;int&gt;*            | INT4RANGE                                    |
| PgRange&lt;DateOnly&gt;*       | DATERANGE                                    |
| PgRange&lt;DateTime&gt;*       | TSRANGE                                      |
| PgRange&lt;DateTimeOffset&gt;* | TSTZRANGE                                    |
| PgRange&lt;decimal&gt;*        | NUMRANGE                                     |
| PgBox*                         | BOX                                          |
| PgCircle*                      | CIRCLE                                       |
| PgInet*                        | INET, CIDR                                   |
| PgInerval*                     | INTERVAL                                     |
| PgLine*                        | LINE                                         |
| PgLineSegment*                 | LSEG                                         |
| PgMacAddress*                  | MACADDR                                      |
| PgMacAddress8*                 | MACADDR8                                     |
| PgMoney*                       | MONEY                                        |
| PgOid*                         | OID                                          |
| PgPath*                        | PATH                                         |
| PgPoint*                       | POINT                                        |
| PgPolygon*                     | POLYGON                                      |
| PgTimeTz*                      | TIME WITH TIME ZONE                          |
| T                              | JSONB, JSON                                  |

\* Type custom to the sqlx-cs-pg library

### JSON
Postgres supports unstructured data through the `JSON` and `JSONB` types. Extracting those field
types are handled by the `IDataRow.GetJson` methods where a generic argument or
`JsonTypeInfo<T>` is provided to tell the row how to deserialize the JSON value. If you require
working with an opaque JSON object then you must specify `JsonNode` as the deserialization type.
Similarly, `IBindable.BindJson` allow for serializing a CLR type or `JsonNode` for sending to the
database.

### Array Types
All postgres types have an implicit array type created and can be extracted as a `T[]` using
`IPgDataRow` methods.

Note that array types are automatically mapped for custom enum and composite types created by a
user when that type if mapped to a connection pool.

### Enum Types
Enum types are natively supported by [Postgres](https://www.postgresql.org/docs/current/datatype-enum.html),
but sometimes you might also want a type that is easier to change then enum types (e.g. removing an
entry can be cumbersome). To accomidate this, sqlx-cs supports CLR `enum` types in 3 ways:
1. Native Postgres enums. Just ensure that `IPgConnectionPool.MapEnumAsync<TEnum>` when initializing
    the pool so that the database specific OID is collected. 
    ```postgresql
    CREATE TYPE enum_type AS ENUM ('none', 'something');
    ```
    ```c#
    [PgEnum(Name = "enum_type", RenameAll = Rename.SnakeCase)]
    public enum EnumType
    {
        None,
        Something,
    }
    ```
2. Int wrapper (simple cast of database `integer` value to an enum value)
    ```c#
    [WrapperEnum(Representation = EnumRepresentation.Int)]
    public enum IntEnum
    {
        None = 0,
        Something = 1,
    }
    ```
3. Text wrapper (uses the enum label names to generate mapping to and from database `text` value)
    ```c#
    [WrapperEnum(Representation = EnumRepresentation.Text)]
    public enum IntEnum
    {
        None,
        [PgName("something")] // Map to slightly different name/value
        Something,
    }
    ```

### Composite Types
[Postgres](https://www.postgresql.org/docs/current/rowtypes.html) also natively supports composite
types that can be declared as a new type with 1 or more attributes that are other postgres types.
Note that this also applies to tables because postgres internally keeps track of table rows as
composite types. This allows you to fetch other complete table rows as the composite and deserialize
those rows (or arrays of rows) on the client rather than sending non-normalized data to the client
and aggregating into the desired objects.
```postgresql
CREATE TYPE composite_type AS (
    id integer,
    "name" text,
    title text
);
```
```c#
[PgComposite(Name = "composite_type", RenameAll = Rename.SnakeCase)]
public readonly partial struct CompositeType
{
    public int Id { get; init; }
    
    public string Name { get; init; }
    
    public string? Title { get; init; }
}
```
Just ensure that `IPgConnectionPool.MapCompositeAsync<TComposite>` when initializing the pool so
that the database specific OID is collected.

## COPY Protocol
[Copy](https://www.postgresql.org/docs/current/sql-copy.html) statements are supported for
`COPY FROM` and `COPY TO`. To execute either statement you must create a `ICopyStatment` instance.
`ICopyStatement`s provide all the features that a raw copy query provides but with some guardrails
to avoid issues (such as specifying the wrong value for options).

To interact with the copy API, there are 4 methods:
- `IPgConnection.CopyOutAsync` => writes the copy data to a provided stream
- `IPgConnection.CopyOutRowsAsync` => transforms binary copy data to row type instances
- `IPgConnection.CopyInAsync` => copies a stream to the connection and returns a `QueryResult` with
the rows affected
- `IPgConnection.CopyInRowsAsync` => consumes a stream of `IPgBinaryCopyRow` instances as copy data
and returns a `QueryResult` with the rows affected

However, there are convenience methods that wrap these base methods to handle common use cases such
as:
- Passing a file as the input data using a file path
- Writing output data to a file

## Listen/Notify
PostgreSQL databases support a [LISTEN](https://www.postgresql.org/docs/current/sql-listen.html)/[NOTIFY](https://www.postgresql.org/docs/current/sql-notify.html)
protocol to subscribe to a desired channel and publish asynchronous messages to subscribers.
Although the `IPgConnection` instances can listen to channel and do receive notifications, they are
not set up to interact with them. There is another type `IPgListener` that uses a connection to
listen and receive notifications. To create a listener use `IPgConnectionPool.CreateListener`. Just
understand that this will remove a connection from the pool for the duration of the listener usage.
```c#
using IPgListener listener = pool.CreateListener();
await listener.ListenAsync("channel");

// Infinite loop until cancelled
await foreach (PgNotification notification in listener.ReceiveNotificationsAsync())
{
    // handle notifcation
}
```
