namespace Sqlx.Core.Query;

/// <summary>
/// <para>
/// Object that stores more than 1 value to bind to an <see cref="IBindable"/>.
/// </para>
/// <para>
/// This is generally used for types that contain parameters bound to a parameterized query. It
/// allows for supplying a group of parameters to a query rather than each parameter 1 by 1.
/// </para>
/// </summary>
/// <typeparam name="TBindable"><see cref="IBindable"/> type to bind to</typeparam>
public interface IBindMany<in TBindable> where TBindable : IBindable
{
    /// <summary>
    /// Bind all values to the <typeparamref name="TBindable"/> instance
    /// </summary>
    /// <param name="bindable">Object to bind values to</param>
    void BindMany(TBindable bindable);
}
