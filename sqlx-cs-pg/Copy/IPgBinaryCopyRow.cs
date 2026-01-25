using Sqlx.Core.Query;
using Sqlx.Postgres.Query;

namespace Sqlx.Postgres.Copy;

/// <summary>
/// Row type that can encode itself into a <see cref="IPgBindable"/> as a copy row. Used to bulk
/// copy a CLR type into a target Postgres table.
/// </summary>
public interface IPgBinaryCopyRow : IBindMany<IPgBindable>
{
    /// <summary>
    /// Number of columns encoded for every row
    /// </summary>
    static abstract short ColumnCount { get; }
}
