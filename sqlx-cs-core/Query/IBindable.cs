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
