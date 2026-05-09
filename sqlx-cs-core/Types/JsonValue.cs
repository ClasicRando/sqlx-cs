namespace Sqlx.Core.Types;

/// <summary>
/// Wrapper for a JSON value. Used to tell the compiler that the database value should be treated
/// as JSON. This means that:
/// <list type="bullet">
///     <item>
///         when binding this value, the driver will serialize to JSON and forward that result to
///         the database
///     </item>
///     <item>
///         when fetching this value, the driver will deserialize the field value into the type
///         <typeparamref name="T"/> and pack that as the <see cref="Inner"/> value.
///     </item>
/// </list>
/// </summary>
/// <typeparam name="T">Internal type to treat as JSON</typeparam>
public readonly record struct JsonValue<T> where T : notnull
{
    public required T Inner { get; init; }
}
