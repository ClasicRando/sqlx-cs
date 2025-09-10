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
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">boolean value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(bool value);

    /// <summary>
    /// Bind boolean parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">boolean value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(bool? value);

    /// <summary>
    /// Bind sbyte parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">byte value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(sbyte value);

    /// <summary>
    /// Bind sbyte parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">byte value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(sbyte? value);

    /// <summary>
    /// Bind short parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">short value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(short value);

    /// <summary>
    /// Bind short parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">short value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(short? value);

    /// <summary>
    /// Bind int parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">int value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(int value);

    /// <summary>
    /// Bind int parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">int value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(int? value);

    /// <summary>
    /// Bind short parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">short value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(long value);

    /// <summary>
    /// Bind long parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">long value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(long? value);

    /// <summary>
    /// Bind long parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">long value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(float value);

    /// <summary>
    /// Bind float parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">float value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(float? value);

    /// <summary>
    /// Bind float parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">float value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(double value);

    /// <summary>
    /// Bind double parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">double value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(double? value);

    /// <summary>
    /// Bind double parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">double value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(TimeOnly value);

    /// <summary>
    /// Bind TimeOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">TimeOnly value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(TimeOnly? value);

    /// <summary>
    /// Bind DateOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">DateOnly value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(DateOnly value);

    /// <summary>
    /// Bind DateOnly parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">DateOnly value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(DateOnly? value);

    /// <summary>
    /// Bind DateTime parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">DateTime value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(DateTime value);

    /// <summary>
    /// Bind DateTime parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">DateTime value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(DateTime? value);

    /// <summary>
    /// Bind DateTimeOffset parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">DateTimeOffset value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(DateTimeOffset value);

    /// <summary>
    /// Bind DateTimeOffset parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">DateTimeOffset value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(DateTimeOffset? value);

    /// <summary>
    /// Bind decimal parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">decimal value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(decimal value);

    /// <summary>
    /// Bind decimal parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">decimal value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(decimal? value);

    /// <summary>
    /// Bind byte[] parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">byte[] value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(byte[]? value);

    /// <summary>
    /// Bind ReadOnlySpan&lt;byte&gt; parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;byte&gt; value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(ReadOnlySpan<byte> value);

    /// <summary>
    /// Bind string parameter to query. This puts that value as the nth parameter in the
    /// parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">string value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(string? value);

    /// <summary>
    /// Bind ReadOnlySpan&lt;char&gt; parameter to query. This puts that value as the nth parameter
    /// in the parameterized query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;char&gt; value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(ReadOnlySpan<char> value);

    /// <summary>
    /// Bind Guid parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">Guid value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(Guid value);

    /// <summary>
    /// Bind Guid parameter to query. This puts that value as the nth parameter in the parameterized
    /// query, where n is the current parameter as a 1-based index.
    /// </summary>
    /// <param name="value">Guid value</param>
    /// <returns>this query instance for method chaining</returns>
    IQuery Bind(Guid? value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="typeInfo"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IQuery BindJson<T>(T? value, JsonTypeInfo<T>? typeInfo = null) where T : notnull;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IQuery BindOutParameter<T>() where T : notnull;
}
