using System.Text;

namespace Sqlx.Core;

/// <summary>
/// Common location for the <see cref="Default"/> charset
/// </summary>
public static class Charsets
{
    /// <summary>
    /// Default character encoding for all driver 
    /// </summary>
    public static readonly Encoding Default = Encoding.UTF8;
}
