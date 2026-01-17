namespace Sqlx.Core.Exceptions;

/// <summary>
/// Special <see cref="SqlxException"/> for when the transaction state of a connection is expected
/// to be specific state, but that is not true. 
/// </summary>
#pragma warning disable CA1032
public sealed class UnexpectedTransactionState : SqlxException
{
    public UnexpectedTransactionState(bool expectedToBeInTransaction)
        : base($"Expected connection to {(expectedToBeInTransaction ? "" : "not ")}be in a transaction") {}
}
