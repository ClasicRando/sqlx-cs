namespace Sqlx.Core;

/// <summary>
/// Type that is either the left or right type but never both or neither. There are exactly 2 nested
/// subclasses that are the only possible states of <see cref="Either{TLeft,TRight}"/> and no other
/// type can extend this since the constructor is private. This is not an extensive implementation
/// of the functional monad but rather a simple implementation for internal purposes.
/// </summary>
/// <typeparam name="TLeft">First possible type</typeparam>
/// <typeparam name="TRight">Second possible type</typeparam>
#nullable disable
public readonly record struct Either<TLeft, TRight>
{
    public Either(TLeft left, TRight right, bool isLeft)
    {
        Left = left;
        Right = right;
        IsLeft = isLeft;
    }

    public bool IsLeft { get; }

    public bool IsRight => !IsLeft;

    public TLeft Left =>
        IsLeft
            ? field
            : throw new InvalidOperationException(
                $"Tried to access {nameof(Left)} when value is {nameof(Right)}");

    public TRight Right => !IsLeft
        ? field
        : throw new InvalidOperationException(
            $"Tried to access {nameof(Right)} when value is {nameof(Left)}");
}

public static class Either
{
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left)
    {
        return new Either<TLeft, TRight>(left, default, true);
    }

    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right)
    {
        return new Either<TLeft, TRight>(default, right, false);
    }
}
