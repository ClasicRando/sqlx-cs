using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Query;

/// <summary>
/// Implementors represent a query to be submitted to the database. It doesn't actually provide the
/// ability to execute the query, but you can get the original query and bind parameters to the
/// query. This means the object must represent a raw query statement and a prepared query statement
/// as a single type. Users should dispose of the query after execution since the bound parameters
/// are encoded into a buffer within the query instance and that buffer can be disposed of to save
/// memory.
/// </summary>
public interface IQuery : IDisposable
{
    /// <summary>
    /// Raw query to submit for execution
    /// </summary>
    string Query { get; }

    /// <summary>
    /// Bind boolean parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. The <c>BOOLEAN</c>
    /// type is not consistent across all databases so the driver specific implementation might
    /// vary.
    /// </summary>
    /// <param name="value">Boolean value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(bool value);

    /// <summary>
    /// Bind sbyte parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TINYINT</c> type but can vary between database implementations since not all use
    /// <c>TINYINT</c>
    /// </summary>
    /// <param name="value">Sbyte value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(sbyte value);

    /// <summary>
    /// Bind short parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>SMALLINT</c> type.
    /// </summary>
    /// <param name="value">Short value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(short value);

    /// <summary>
    /// Bind int parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index. This maps to the <c>INTEGER</c>
    /// type.
    /// </summary>
    /// <param name="value">Int value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(int value);

    /// <summary>
    /// Bind long parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>BIGINT</c> type.
    /// </summary>
    /// <param name="value">Long value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(long value);

    /// <summary>
    /// Bind float parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>REAL</c> type.
    /// </summary>
    /// <param name="value">Float value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(float value);

    /// <summary>
    /// Bind double parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>DOUBLE PRECISION</c> type.
    /// </summary>
    /// <param name="value">Double value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(double value);

    /// <summary>
    /// Bind TimeOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TIME</c> type.
    /// </summary>
    /// <param name="value">TimeOnly value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(TimeOnly value);

    /// <summary>
    /// Bind DateOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>DATE</c> type.
    /// </summary>
    /// <param name="value">DateOnly value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(DateOnly value);

    /// <summary>
    /// Bind DateTime parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TIMESTAMP</c> type.
    /// </summary>
    /// <param name="value">DateTime value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(DateTime value);

    /// <summary>
    /// Bind DateTimeOffset parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TIMESTAMP WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="value">DateTimeOffset value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(DateTimeOffset value);

    /// <summary>
    /// Bind decimal parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>DECIMAL</c> type.
    /// </summary>
    /// <param name="value">Decimal value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(decimal value);

    /// <summary>
    /// Bind byte[] parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>VARBINARY</c>/<c>BLOB</c> type.
    /// </summary>
    /// <param name="value">Byte[] value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(byte[]? value);

    /// <summary>
    /// Bind ReadOnlySpan&lt;byte&gt; parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the <c>VARBINARY</c>/<c>BLOB</c> type.
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;byte&gt; value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(ReadOnlySpan<byte> value);

    /// <summary>
    /// Bind string parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
    /// </summary>
    /// <param name="value">String value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(string? value);

    /// <summary>
    /// Bind ReadOnlySpan&lt;char&gt; parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index. This maps
    /// to the <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type.
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;char&gt; value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(ReadOnlySpan<char> value);

    /// <summary>
    /// Bind Guid parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index. The
    /// <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is not consistent across all databases so the
    /// driver specific implementation might vary. Generally it's either a built-in type or this
    /// method tries to interpret a <see cref="Guid"/> as bytes or a string.
    /// </summary>
    /// <param name="value">Guid value</param>
    /// <returns>This query instance for method chaining</returns>
    IQuery Bind(Guid value);

    /// <summary>
    /// Bind <typeparamref name="T"/> parameter to query as a JSON. This puts that value as the nth
    /// parameter in the parameterized query, where n is the current parameter as a 1-based index.
    /// Some databases have a JSON specific field type but other database drivers will treat the
    /// JSON encoding as string or bytes. When using this method, it's recommended to supply the
    /// <see cref="JsonTypeInfo{T}"/> parameter to aid serialization.
    /// </summary>
    /// <param name="value">Value to encode as JSON</param>
    /// <param name="typeInfo">Optional type metadata for JSON serialization</param>
    /// <typeparam name="T">CLR type to encode as JSON</typeparam>
    /// <returns>This query instance for method chaining</returns>
    IQuery BindJson<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : notnull;

    /// <summary>
    /// Bind a null value to the query. This puts a null as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index;
    /// </summary>
    /// <typeparam name="T">
    /// CLR type to hint the driver as to the parameter's expected type. Drivers may or may not use
    /// this type to inform query preparing.
    /// </typeparam>
    /// <returns></returns>
    IQuery BindNull<T>() where T : notnull;
}

public static class Query
{
    /// <summary>
    /// Wrapper method for specifying a parameter that is intended to be an <c>OUT</c> only
    /// parameter in a stored procedure call. This is equivalent to <see cref="IQuery.BindNull"/>
    /// since an <c>OUT</c> parameter always has an input value of <c>NULL</c>. Use this method to
    /// indicate that the parameter's output will be captured in the query result.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <typeparam name="T">
    /// <c>OUT</c> parameter's CLR type to hint the driver as to the parameter's expected type.
    /// Drivers may or may not use this type to inform query preparing.
    /// </typeparam>
    /// <returns>This query instance for method chaining</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQuery BindOutParameter<T>(this IQuery query) where T : notnull
    {
        return query.BindNull<T>();
    }
    
    /// <summary>
    /// Bind boolean parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. The <c>BOOLEAN</c>
    /// type is not consistent across all databases so the driver specific implementation might
    /// vary.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Boolean value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, bool? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<bool>();
    }

    /// <summary>
    /// Bind sbyte parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TINYINT</c> type but can vary between database implementations since not all use
    /// <c>TINYINT</c>
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Sbyte value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, sbyte? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<sbyte>();
    }

    /// <summary>
    /// Bind short parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>SMALLINT</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Short value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, short? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<short>();
    }

    /// <summary>
    /// Bind int parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index. This maps to the <c>INTEGER</c>
    /// type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Int value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, int? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<int>();
    }

    /// <summary>
    /// Bind long parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>BIGINT</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Long value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, long? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<long>();
    }

    /// <summary>
    /// Bind float parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>REAL</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Float value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, float? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<float>();
    }

    /// <summary>
    /// Bind double parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>DOUBLE PRECISION</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Double value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, double? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<double>();
    }

    /// <summary>
    /// Bind TimeOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TIME</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">TimeOnly value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, TimeOnly? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<TimeOnly>();
    }

    /// <summary>
    /// Bind DateOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>DATE</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">DateOnly value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, DateOnly? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<DateOnly>();
    }

    /// <summary>
    /// Bind DateTime parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TIMESTAMP</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">DateTime value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, DateTime? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<DateTime>();
    }

    /// <summary>
    /// Bind DateTimeOffset parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>TIMESTAMP WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">DateTimeOffset value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, DateTimeOffset? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<DateTimeOffset>();
    }

    /// <summary>
    /// Bind decimal parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index. This maps to the
    /// <c>DECIMAL</c> type.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Decimal value</param>
    /// <returns>This query instance for method chaining</returns>
    public static IQuery Bind(this IQuery query, decimal? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<decimal>();
    }

    /// <summary>
    /// Bind Guid parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index. The
    /// <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is not consistent across all databases so the
    /// driver specific implementation might vary. Generally it's either a built-in type or this
    /// method tries to interpret a <see cref="Guid"/> as bytes or a string.
    /// </summary>
    /// <param name="query">Query to execute the bind operation against</param>
    /// <param name="value">Guid value</param>
    /// <returns>This query instance for method chaining</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IQuery Bind(this IQuery query, Guid? value)
    {
        return value.HasValue ? query.Bind(value.Value) : query.BindNull<Guid>();
    }
}
