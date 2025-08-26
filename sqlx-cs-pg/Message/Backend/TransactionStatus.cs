namespace Sqlx.Postgres.Message.Backend;

public enum TransactionStatus : byte
{
    Idle = (byte)'I',
    InTransaction = (byte)'T',
    FailedTransaction = (byte)'E',
}
