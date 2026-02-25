# SQLx C#
## Overview
.NET Library for database access that leverages new features in C# to improve upon the ADO.NET API.
The aim of the library is to include other databases with open/available client protocol
documentation.

## Core
### Basic Query
```c#
struct Row : IFromRow<DbRow, Row>
{
    public required int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required int? ChildId { get; init; }
    
    public static Row FromRow(DbRow dataRow)
    {
        return new Row
        {
            Id = dataRow.GetIntNotNull("id"),
            Name = dataRow.GetStringNotNull("name"),
            ChildId = dataRow.GetInt("child_id"),
        };
    }
}

// Full usage
await using var connection = pool.CreateConnection();
using var query = connection.CreateQuery(sql);
var rows = await query.FetchAllAsync<Row>();

// With no query
// Creates query, executes query to fetch all and disposes of the query
await using var connection = pool.CreateConnection();
var rows = await connection.FetchAllAsync<Row>(sql);

// With no connection
// Create connection, create query, executes query to fetch all and disposes of connection and query
var rows = await pool.FetchAllAsync<Row>(sql);
```

### Parameterized Query
```c#
struct Param : IBindMany<DbBindable>
{
    public required int Id { get; init; }
    
    public void BindMany(DbBindable bindable)
    {
        bindable.Bind(Id);
    }
}

// Full usage with manual binding
await using var connection = pool.CreateConnection();
using var query = connection.CreateQuery(parameterizedQuery);
// Binds to first parameter present in query
query.Bind(1); 
var rows = await query.FetchAllAsync<Row>();

// With no query and param type
await using var connection = pool.CreateConnection();
var rows = await connection.FetchAllAsync<Param, Row>(sql, new Param { Id = 1 });

// With no connection or query and param type
var rows = await pool.FetchAllAsync<Param, Row>(sql, new Param { Id = 1 });
```

### Batch Queries
```c#
// With query batch, manually binding parameters
await using var connection = pool.CreateConnection();
using var queryBatch = connection.CreateQueryBatch();

Param[] paramBatches;
foreach (Param param in paramBatches)
{
    var query = queryBatch.CreateQuery(insertSql);
    param.BindMany(query);
}

var rowsAffected = await queryBatch.ExecuteNonQueryAsync();

// With extension method
await using var connection = pool.CreateConnection();
Param[] paramBatches;
var rowsAffected = await connection.ExecuteNonQueryBatchAsync(insertSql, paramBatches);
```

### FromRow
`IFromRow` allows for defining how data row deserialization is facilitated to create a new instance
of the type that implements this interface. For examples of its usage, see the code blocks above.
The interface can be implemented manually or source generated. The source generated option is
implemented for each database and might provide different options. Example of a database
implementation:
```c#
[Sqlx.Postgres.Generated.FromRow]
partial struct Row
{
    public required int Id { get; init; }
    
    public required string Name { get; init; }
    
    public required int? ChildId { get; init; }
}
```

## Philosophy
### Implement General Use Cases as Extension Methods
When interfacing with databases, a few use cases/patterns show up WAY more than others. Rather
than ask others to handle that duplication themselves through extension members or god forbid
copy and paste code, extension members are made available on the library level. For example, in a
lot of cases we execute a parameterized query with 1 result set and map those rows to a type. Other
libraries require the ceremony of:
1. Creating a command
2. Binding parameters
3. Executing a reader
4. Iterating over that reader
5. Mapping each row as a new type

This is fine and there are libraries on top of the drivers that make this much easier, but we would
rather have that behaviour provided for you to make the interaction with the DB streamlined. As long
as the row type has `IFromRow` implemented for itself against your database, you can simply call,
`await connection.FetchAll<RowType(sql)`. There is also a case for handling parameterized queries
where you implement `IBindAll` for your database on a parameter type so that you call
`await connection.FetchAll<ParamType, RowType>(sql, param)`. This has the downside of obfuscating
the actual row parsing and parameter binding to implementations but it at least makes it explicit.

### Result Mapping
Although sqlx-cs can be used to deal with dynamic result sets and row shapes, it thrives when row
types are known and clearly defined as CLR types. Each database provides a source generator for
creating implementations of `IFromRow` for that database and the type itself. Developers should lean
on that functionality rather than trying to deal with rows as opaque tuples.

### Embracing new C# features
The original intention of the library was to make an ADO.NET with modern features of the language.
Although some features such as:
- Using `IAsyncEnumerable` for result sets
- Using `PipeReader` on the connection stream

were dropped due to performance concerns, the library tries to set itself up for future additions
to the language such as:
- Extension interfaces
- Sealed enums
- Union types

### Opinionated Design
You might notice that there are some core module interfaces for connections, queries or data rows.
You might have also noticed that they have complex generics types associated with them.

THIS IS BY DESIGN.

This means that you will have a hard time interacting with the library as though every connection,
query or data row is essentially the same thing even though they are linked to separate databases.
It's this library's opinion that generic repositories are not the best solution and should only be
chosen with care and concern for the downsides. You might notice similar takes on other options or
functionality in this library where certain patterns are discouraged but not impossible. This is not
to say that someone cannot make that decision, it's just that the author(s) of this library do not
want to encourage to make certain decisions. If you would like fewer restrictions like this, ADO.NET
would be an easier option.

## Database Support
### [Postgres](sqlx-cs-pg/Postgres.md)

### Connection Encryption
SSL connections are not implemented.... yet!

### FAQs
#### Is there support for non-async/blocking connections?
No. Database interactions generally involve some sort of IO so it would be best to put that into an
async operation. However, if there is enough desire to use this library for blocking connections
than it could be a future addition.

#### Why does everything involve extension methods?
As mentioned [here](#implement-general-use-cases-as-extension-methods), extension methods are the
best way to add behaviour and compose multiple concepts/types into a single general use case.

There are also cases where extension methods may seem a little overkill such as row field
deserialization. Honestly, I agree that it's a little convoluted, but it's the only way to add the
ability to deserialize certain types without making a general `IDataRow.Get<T>`. These types of
generic methods without bounds are rife for developers to add values that can never be deserialized,
and you won't know until you run your application. With that in mind, we hope to remove those
methods when this [feature](https://github.com/dotnet/csharplang/issues/9319) is finally implemented
in C#.
