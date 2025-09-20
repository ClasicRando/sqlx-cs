namespace Sqlx.Postgres.Message.Frontend;

/// <summary>
/// Target of a <see cref="PgFrontendMessageType.Close"/> and
/// <see cref="PgFrontendMessageType.Describe"/> message.
/// </summary>
public enum MessageTarget : byte
{
    PreparedStatement = (byte)'S',
    Portal = (byte)'P',
}
