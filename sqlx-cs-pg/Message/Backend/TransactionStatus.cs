namespace Sqlx.Postgres.Message.Backend;

/// <summary>
/// Transaction status code sent by the backend within the <see cref="ReadyForQueryMessage"/>. This
/// indicates if a transaction is still active once completing the query or if the current
/// transaction failed and should be rolled back.
/// </summary>
internal enum TransactionStatus
{
    Idle = (byte)'I',
    InTransaction = (byte)'T',
    FailedTransaction = (byte)'E',
}
