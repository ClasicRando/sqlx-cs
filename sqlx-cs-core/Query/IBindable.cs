using System.Text.Json.Serialization.Metadata;

namespace Sqlx.Core.Query;

/// <summary>
/// Implementors represent a type that can bind values to itself for use in parameterized queries
/// and database specific UDTs. Users should dispose of the query after execution since the bound
/// parameters are encoded into a buffer within the query instance and that buffer can be disposed
/// of to save/reuse memory.
/// </summary>
public interface IBindable : IDisposable
{
    /// <summary>
    /// Bind a boolean value. The <c>BOOLEAN</c> type is not consistent across all databases so the
    /// driver specific implementation might vary.
    /// </summary>
    /// <param name="value">Boolean value</param>
    void Bind(bool value);

    /// <summary>
    /// Bind a sbyte value. This maps to the <c>TINYINT</c> type but can vary between database
    /// implementations since not all use <c>TINYINT</c>
    /// </summary>
    /// <param name="value">Sbyte value</param>
    void Bind(sbyte value);

    /// <summary>
    /// Bind a short value. This maps to the <c>SMALLINT</c> type.
    /// </summary>
    /// <param name="value">Short value</param>
    void Bind(short value);

    /// <summary>
    /// Bind an int value. This maps to the <c>INTEGER</c> type.
    /// </summary>
    /// <param name="value">Int value</param>
    void Bind(int value);

    /// <summary>
    /// Bind a long value. This maps to the <c>BIGINT</c> type.
    /// </summary>
    /// <param name="value">Long value</param>
    void Bind(long value);

    /// <summary>
    /// Bind a float value. This maps to the <c>REAL</c> type.
    /// </summary>
    /// <param name="value">Float value</param>
    void Bind(float value);

    /// <summary>
    /// Bind a double value. This maps to the <c>DOUBLE PRECISION</c> type.
    /// </summary>
    /// <param name="value">Double value</param>
    void Bind(double value);

    /// <summary>
    /// Bind a TimeOnly value. This maps to the <c>TIME</c> type.
    /// </summary>
    /// <param name="value">TimeOnly value</param>
    void Bind(TimeOnly value);

    /// <summary>
    /// Bind a DateOnly value. This maps to the <c>DATE</c> type.
    /// </summary>
    /// <param name="value">DateOnly value</param>
    void Bind(DateOnly value);

    /// <summary>
    /// Bind a DateTime value. This maps to the <c>TIMESTAMP</c> type.
    /// </summary>
    /// <param name="value">DateTime value</param>
    void Bind(DateTime value);

    /// <summary>
    /// Bind a DateTimeOffset value. This maps to the <c>TIMESTAMP WITH TIME ZONE</c> type.
    /// </summary>
    /// <param name="value">DateTimeOffset value</param>
    void Bind(in DateTimeOffset value);

    /// <summary>
    /// Bind a decimal value. This maps to the <c>DECIMAL</c>/<c>NUMERIC</c> type.
    /// </summary>
    /// <param name="value">Decimal value</param>
    void Bind(decimal value);

    /// <summary>
    /// Bind a byte[] value. This maps to the <c>VARBINARY</c>/<c>BLOB</c> type although database
    /// implementations might have custom types (e.g. Postgres has <c>BYTEA</c>)
    /// </summary>
    /// <param name="value">Byte[] value</param>
    void Bind(byte[]? value);

    /// <summary>
    /// Bind a byte[] value. This maps to the <c>VARBINARY</c>/<c>BLOB</c> type although database
    /// implementations might have custom types (e.g. Postgres has <c>BYTEA</c>)
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;byte&gt; value</param>
    void Bind(in ReadOnlySpan<byte> value);

    /// <summary>
    /// Bind a string value. This maps to the <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type although
    /// database implementations might have custom character types (e.g. Postgres has
    /// <c>CITEXT</c>).
    /// </summary>
    /// <param name="value">String value</param>
    void Bind(string? value);

    /// <summary>
    /// Bind a string value. This maps to the <c>VARCHAR</c>/<c>TEXT</c>/<c>CLOB</c> type although
    /// database implementations might have custom character types (e.g. Postgres has
    /// <c>CITEXT</c>).
    /// </summary>
    /// <param name="value">ReadOnlySpan&lt;char&gt; value</param>
    void Bind(in ReadOnlySpan<char> value);

    /// <summary>
    /// Bind a Guid value. The <c>UUID</c>/<c>UNIQUEIDENTIFIER</c> type is not consistent across all
    /// databases so the driver specific implementation might vary. Generally it's either a built-in
    /// type or this method tries to interpret a <see cref="Guid"/> as bytes or a string.
    /// </summary>
    /// <param name="value">Guid value</param>
    void Bind(in Guid value);

    /// <summary>
    /// Bind a <typeparamref name="T"/> value as a JSON. Some databases have a JSON specific field
    /// type but other database drivers will treat the JSON encoding as string or bytes. When using
    /// this method, it's recommended to supply the <see cref="JsonTypeInfo"/> parameter to aid
    /// serialization. If you need to bind a possibly null value, use
    /// <see cref="Bindable.BindJsonRef"/> and <see cref="Bindable.BindJsonVal"/> for class and
    /// struct types respectively.
    /// </summary>
    /// <param name="value">Value to encode as JSON</param>
    /// <param name="typeInfo">Optional type metadata for JSON serialization</param>
    /// <typeparam name="T">CLR type to encode as JSON</typeparam>
    void BindJson<T>(T value, JsonTypeInfo<T>? typeInfo = null) where T : notnull;

    /// <summary>
    /// Bind a null value to the query
    /// </summary>
    /// <typeparam name="T">
    /// CLR type to hint the driver as to the parameter's expected type. Drivers may or may not use
    /// this type to inform query preparing.
    /// </typeparam>
    void BindNull<T>() where T : notnull;
}
