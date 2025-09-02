namespace Sqlx.Core;

public abstract record Either<TLeft, TRight>
{
    private Either()
    {
    }

    public sealed record Left(TLeft Value) : Either<TLeft, TRight>;
    public sealed record Right(TRight Value) : Either<TLeft, TRight>;
}
