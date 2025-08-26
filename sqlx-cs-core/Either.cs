namespace Sqlx.Core;

public class Either<TLeft, TRight>
{
    public TLeft? Left { get; }
    public TRight? Right { get; }
    
    private Either(TLeft? left, TRight? right)
    {
        Left = left;
        Right = right;
    }

    public static Either<TLeft, TRight> OfLeft(TLeft left)
    {
        return new Either<TLeft, TRight>(left, default);
    }

    public static Either<TLeft, TRight> OfRight(TRight right)
    {
        return new Either<TLeft, TRight>(default, right);
    }
}
