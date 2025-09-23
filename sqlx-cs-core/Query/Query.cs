using System.Runtime.CompilerServices;

namespace Sqlx.Core.Query;

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
