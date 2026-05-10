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
    /// Bind a null value to the query
    /// </summary>
    /// <typeparam name="T">
    /// CLR type to hint the driver as to the parameter's expected type. Drivers may or may not use
    /// this type to inform query preparing.
    /// </typeparam>
    void BindNull<T>() where T : notnull;
}
