using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// Row type that can encode itself into a <see cref="IPgBindable"/> as a copy row. Used to bulk
/// copy a CLR type into a target Postgres table. 
/// </summary>
public interface IPgBinaryCopyRow
{
    /// <summary>
    /// Number of columns encoded for every row
    /// </summary>
    static abstract short ColumnCount { get; }
    
    /// <summary>
    /// Encode this instance into the supplied <see cref="IPgBindable"/>
    /// </summary>
    /// <param name="bindable">Encoding bindable</param>
    void BindValues(IPgBindable bindable);
}
