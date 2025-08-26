namespace Sqlx.Postgres.Message.Frontend;

public enum MessageTarget : byte
{
    PreparedStatement = (byte)'S',
    Portal = (byte)'P',
}
