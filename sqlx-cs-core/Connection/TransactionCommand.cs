namespace Sqlx.Core.Connection;

public enum TransactionCommand
{
    Begin = 0,
    Commit = 1,
    Rollback = 2,
}
