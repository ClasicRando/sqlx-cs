using System.Text.Json;

namespace Sqlx.Postgres.Type;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPgComposite<T> : IPgUdt<T> where T : notnull
{
    /// <summary>
    /// 
    /// </summary>
    static abstract JsonSerializerOptions JsonOptions { get; set; }
}
