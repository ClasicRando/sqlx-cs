namespace Sqlx.Core.Exceptions;

public class UnexpectedTransactionState : SqlxException
{
    public UnexpectedTransactionState(bool expectedToBeInTransaction)
        : base($"Expected connection to {(expectedToBeInTransaction ? "" : "not ")}be in a transaction") {}
}
