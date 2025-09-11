namespace Sqlx.Core;

/// <summary>
/// Type that is either the left or right type but never both or neither. There are exactly 2 nested
/// subclasses that are the only possible states of <see cref="Either{TLeft,TRight}"/> and no other
/// type can extend this since the constructor is private. This is not an extensive implementation
/// of the functional monad but rather a simple implementation for internal purposes.
/// </summary>
/// <typeparam name="TLeft">First possible type</typeparam>
/// <typeparam name="TRight">Second possible type</typeparam>
public abstract record Either<TLeft, TRight>
{
    private Either()
    {
    }

    /// <summary>
    /// The first variant of <see cref="Either{TLeft,TRight}"/> that represents an instance of the
    /// left type.
    /// </summary>
    /// <param name="Value">Inner value</param>
    public sealed record Left(TLeft Value) : Either<TLeft, TRight>;
    /// <summary>
    /// The second variant of <see cref="Either{TLeft,TRight}"/> that represents an instance of the
    /// right type.
    /// </summary>
    /// <param name="Value">Inner value</param>
    public sealed record Right(TRight Value) : Either<TLeft, TRight>;
}
